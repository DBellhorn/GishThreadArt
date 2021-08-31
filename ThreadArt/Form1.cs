using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ThreadArt
{
	public partial class Form1 : Form
	{
		float threadDiameter = 0.1f;

		int nailCount = 180;
		float nailDiameter = 1.5f;
		float nailRingDiameter = 600.0f;
		float artBorder = 10.0f;
		int maxLineCount = 5000;

		int totalPixels = 1;
		float pixelSize = 1.0f;

		int threadDensity = 4;

		// Check this number of pixels on either side of lines during calculations
		int lineOffsetCheck = 1;

		string resultsFilename = @"E:\Projects\ThreadArt\ThreadArt\statistics.json";
		List<ResultData> resultList;

		Nail[] nails;
		PointF[] simulatedNails;

		bool[][] nailAvailability;
		Pixel[,] artPixels;

		// TODO: Make all the colors user controllable!
		Color previewBackgroundColor = Color.LightCyan;
		Color previewCircleColor = Color.White;
		Color previewLineColor = Color.Black;

		Bitmap sourceImage;
		string sourceImagePath = "";

		Bitmap previewImage;
		string previewImagePath = "ThreadPreview.jpeg";
		System.Drawing.Imaging.ImageFormat previewImageFormat = System.Drawing.Imaging.ImageFormat.Jpeg;
		string lineFilePath = "NailPattern.txt";

		List<int> linePoints;

		// The following variables are used to display an in-progress image during
		// processing.  Not sure if this is ideal, but seems to work as required.
		List<int> partialLinePoints;
		Mutex accessingLinePoints = new Mutex();
		Mutex accessingPartialPreviewImage = new Mutex();
		Bitmap partialPreviewImage;
		float simulatedNailRadius = 1.7f;
		System.Windows.Forms.Timer partialPreviewDrawTimer;

		float zeroThreshold = 0.0001f;

		bool writePerformanceOutput = false;

		public Form1()
		{
			InitializeComponent();

			// Nail counts are based on easily divisible degree seperations which
			// is absolutely unneccessary.  Switching to a text box for arbitrary
			// amounts is probably safe, but have not had time for testing.
			cbNailCount.Items.Add("36");
			cbNailCount.Items.Add("40");
			cbNailCount.Items.Add("45");
			cbNailCount.Items.Add("60");
			cbNailCount.Items.Add("72");
			cbNailCount.Items.Add("90");
			cbNailCount.Items.Add("120");
			cbNailCount.Items.Add("180");
			cbNailCount.Items.Add("240");
			cbNailCount.Items.Add("360");
			cbNailCount.SelectedIndex = 8;

			string defaultSourceImage = @"E:\Projects\ThreadArt\ThreadArt\images\castiel_4.bmp";
			if (File.Exists(defaultSourceImage))
			{
				SetSourceImagePath(defaultSourceImage);
			}
			btnSimulate.Enabled = true;

			LoadResultsData();
		}

		private void btnOpenImage_Click(object sender, EventArgs e)
		{
			OpenFileDialog dlgFile = new OpenFileDialog();
			dlgFile.Filter = "Image files|*.bmp;*.gif;*.exif;*.jpg;*.jpeg;*.png;*.tiff";
			dlgFile.Filter += "|BMP|*.bmp|GIF|*.gif|EXIF|*.exif|JPEG|*.jpg;*.jpeg|TIFF|*.tiff";

			if (tbImagePath.Text.Length > 0)
			{
				try
				{
					dlgFile.InitialDirectory = Path.GetDirectoryName(tbImagePath.Text);
				}
				catch (Exception ex)
				{
					Debug.WriteLine("Error while parsing source image path: " + ex.Message);
				}
			}

			if (DialogResult.OK == dlgFile.ShowDialog())
			{
				SetSourceImagePath(dlgFile.FileName);

				// Clean up any image we had previously loaded
				if (null != sourceImage)
				{
					sourceImage.Dispose();
					sourceImage = null;
				}

				// Try to load the newly specified image
				if (dlgFile.FileName.Length > 0)
				{
					if (File.Exists(dlgFile.FileName))
					{
						try
						{
							sourceImage = new Bitmap(dlgFile.FileName);

							// Ensure the user can begin processing the image now that it exists
							btnSimulate.Enabled = true;
						}
						catch (FileNotFoundException)
						{
							MessageBox.Show("The source image file was not found.", "Source Image", MessageBoxButtons.OK, MessageBoxIcon.Error);
						}
						catch (Exception)
						{
							MessageBox.Show("An error occurred while attempting to open the source image.", "Source Image", MessageBoxButtons.OK, MessageBoxIcon.Error);
						}
					}
					else
					{
						MessageBox.Show("Unable to locate the specified source image.", "Source Image", MessageBoxButtons.OK, MessageBoxIcon.Information);
					}
				}
				else
				{
					MessageBox.Show("You need to specify a source image.", "Source Image", MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
			}
		}

		private async void btnSimulate_Click(object sender, EventArgs e)
		{
			EnableUI(false);

			if (null == sourceImage)
			{
				// Try to load the newly specified image
				if (tbImagePath.Text.Length > 0)
				{
					if (File.Exists(tbImagePath.Text))
					{
						try
						{
							sourceImage = new Bitmap(tbImagePath.Text);
						}
						catch (FileNotFoundException)
						{
							MessageBox.Show("The source image file was not found.", "Source Image", MessageBoxButtons.OK, MessageBoxIcon.Error);
						}
						catch (Exception)
						{
							MessageBox.Show("An error occurred while attempting to open the source image.", "Source Image", MessageBoxButtons.OK, MessageBoxIcon.Error);
						}
					}
					else
					{
						MessageBox.Show("Unable to locate the specified source image.", "Source Image", MessageBoxButtons.OK, MessageBoxIcon.Information);
					}
				}
				else
				{
					MessageBox.Show("You need to specify a source image.", "Source Image", MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
			}

			if (null != sourceImage)
			{
				// Keep track of how long it takes to process the source image
				ResultData currentTest = new ResultData();
				currentTest.Id = (null != resultList) ? resultList.Count : 0;
				currentTest.StartTime = DateTime.Now;

				InitializeArtwork();

				// Start the preview image drawing timer
				partialPreviewDrawTimer = new System.Windows.Forms.Timer();
				partialPreviewDrawTimer.Interval = 100;
				partialPreviewDrawTimer.Tick += OnDrawPartialPreviewTimer;
				partialPreviewDrawTimer.Start();

				currentTest.LineCount = await CreateArtworkAsync();

				// Release the preview image drawing timer now that we can generate a final version
				partialPreviewDrawTimer.Stop();
				partialPreviewDrawTimer.Dispose();
				partialPreviewDrawTimer = null;

				// Also no need to hold onto the image used to display progress
				if (null != partialPreviewImage)
				{
					accessingPartialPreviewImage.WaitOne();
					partialPreviewImage.Dispose();
					partialPreviewImage = null;
					accessingPartialPreviewImage.ReleaseMutex();
				}

				if (currentTest.LineCount > 0)
				{
					GenerateSimulatedImage();

					if (writePerformanceOutput)
					{
						currentTest.EndTime = DateTime.Now;
						TimeSpan testDuration = currentTest.EndTime - currentTest.StartTime;
						currentTest.TotalSeconds = testDuration.TotalSeconds;
						currentTest.TotalMinutes = testDuration.TotalMinutes;
						currentTest.TotalHours = testDuration.TotalHours;

						if (null == resultList)
						{
							resultList = new List<ResultData>();
						}
						resultList.Add(currentTest);

						WriteResultsData();

						if (null != previewImage)
						{
							String imageFilename = @"E:\Projects\ThreadArt\ThreadArt\images\test";
							imageFilename += currentTest.Id.ToString("D5");
							imageFilename += ".jpeg";

							previewImage.Save(imageFilename, System.Drawing.Imaging.ImageFormat.Jpeg);
						}
					}
				}

				// Force our canvas to redraw
				panelCanvas.Refresh();
			}

			EnableUI(true);
		}

		private void panelCanvas_Paint(object sender, PaintEventArgs e)
		{
			// Don't overwrite the calculation progress view
			if (null == partialPreviewImage)
			{
				using (Graphics g = panelCanvas.CreateGraphics())
				{
					Point canvasCenter = new Point(panelCanvas.Width / 2, panelCanvas.Height / 2);
					int circleDiameter = Math.Min(panelCanvas.Height, panelCanvas.Width);
					int circleRadius = circleDiameter / 2;

					// Color in our entire background to match the preview image
					SolidBrush backgroundBrush = new SolidBrush(previewBackgroundColor);
					g.FillRectangle(backgroundBrush, 0, 0, panelCanvas.Width, panelCanvas.Height);

					GenerateSimulatedImage();
					if (null != previewImage && null != linePoints)
					{
						// Scale the preview image to fit within our UI's panel
						g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
						g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
						g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
						g.DrawImage(previewImage,
									canvasCenter.X - circleRadius,
									canvasCenter.Y - circleRadius,
									circleDiameter,
									circleDiameter);

						// Display how many lines were generated during this simulation
						int lineCount = linePoints.Count - 1;
						String drawString = lineCount.ToString() + " Lines";

						// Create font and brush.
						Font drawFont = new Font("Arial", 10);
						SolidBrush drawBrush = new SolidBrush(Color.Black);

						// Create point for upper-left corner of drawing.
						PointF drawPoint = new PointF(5.0f, 5.0f);

						// Draw string to screen.
						g.DrawString(drawString, drawFont, drawBrush, drawPoint);

						SizeF stringSize = g.MeasureString(drawString, drawFont);
						drawPoint.Y += 1.25f * stringSize.Height;
						drawString = CalculateThreadLength().ToString() + " Meters";
						g.DrawString(drawString, drawFont, drawBrush, drawPoint);
					}
					else
					{
						// Draw a white circle as our image background
						Brush circleBrush = new SolidBrush(previewCircleColor);
						g.FillEllipse(circleBrush, canvasCenter.X - circleRadius, canvasCenter.Y - circleRadius, circleDiameter, circleDiameter);
					}
				}
			}
		}

		private void LoadResultsData()
		{
			if (File.Exists(resultsFilename))
			{
				string jsonString = File.ReadAllText(resultsFilename);
				resultList = JsonSerializer.Deserialize<List<ResultData>>(jsonString);
			}
		}

		private void WriteResultsData()
		{
			if (null != resultList)
			{
				var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
				File.WriteAllText(resultsFilename, JsonSerializer.Serialize(resultList, jsonOptions));
			}
		}

		private void SetSourceImagePath(string imagePath)
		{
			sourceImagePath = imagePath;
			tbImagePath.Text = sourceImagePath;
			tbImagePath.Select(sourceImagePath.Length, 0);
			tbImagePath.ScrollToCaret();
		}

		private void InitializeArtwork()
		{
			if (sourceImage != null)
			{
				UpdateUserParameters();

				float nailRingRadius = nailRingDiameter / 2.0f;
				float artDiameter = nailRingDiameter - (artBorder * 2.0f);

				// Focus on a square area covering the shortest dimension of our source image
				int pixelCount = Math.Min(sourceImage.Height, sourceImage.Width);

				// Calculate the pixel size of our source image based on the available space
				float imagePixelSize = artDiameter / (float)pixelCount;

				// Determine how many pixels the border contains based on that value
				int borderPixels = (int)Math.Round(artBorder / imagePixelSize);

				// Finally we can utilize this information to determine a total number of pixels
				// across the entire working space, and then a correspondingly accurate pixel size.
				totalPixels = pixelCount + (2 * borderPixels);
				pixelSize = nailRingDiameter / (float)totalPixels;

				// Probably a more efficient way to handle this, but this works for now
				// Estimate which pixel is located at the center of everything
				float centerLocation = nailRingDiameter / 2.0f;

				float artRadiusSquared = (artDiameter / 2.0f) * (artDiameter / 2.0f);

				float pixelOffset = pixelSize / 2.0f;
				float pixelRadiusSquared = (pixelOffset * pixelOffset) + (pixelOffset * pixelOffset);
				float pixelRadius = (float)Math.Sqrt(pixelRadiusSquared);

				// Initialize all of our pixels
				artPixels = new Pixel[totalPixels, totalPixels];
				for (int x = 0; x < totalPixels; ++x)
				{
					for (int y = 0; y < totalPixels; ++y)
					{
						double locationX = pixelOffset + (pixelSize * (double)x);
						double locationY = pixelOffset + (pixelSize * (double)y);

						// Only pixels inside our border perimeter are considered during later calculations
						double distX = centerLocation - locationX;
						double distY = centerLocation - locationY;
						bool isValid = ((distX * distX) + (distY * distY)) < artRadiusSquared;

						// Only use pixels within the specified boundary area
						artPixels[x, y] = new Pixel(new PointF((float)locationX, (float)locationY), isValid);
					}
				}

				// Copy data from the source image into our pixel representations
				int imageOffsetX = (sourceImage.Width > pixelCount) ? ((sourceImage.Width - pixelCount) / 2) : 0;
				int imageOffsetY = (sourceImage.Height > pixelCount) ? ((sourceImage.Height - pixelCount) / 2) : 0;

				for (int x = 0; x < pixelCount; ++x)
				{
					for (int y = 0; y < pixelCount; ++y)
					{
						artPixels[x + borderPixels, y + borderPixels].SetStartingColor(sourceImage.GetPixel(x + imageOffsetX, y + imageOffsetY));
					}
				}

				// Calculate how large an image is required for 1 pixel to equal our thread's diameter
				float threadsPerPixel = pixelSize / threadDiameter;
				float imageDiameter = threadsPerPixel * (float)totalPixels;
				float ringDiameter = threadsPerPixel * (float)totalPixels;
				float ringRadius = ringDiameter / 2.0f;
				float imageCenter = (ringDiameter + (nailDiameter * threadsPerPixel)) / 2.0f;
				int imageSize = (int)Math.Round(imageCenter * 2.0f);

				simulatedNailRadius = nailDiameter * threadsPerPixel / 2.0f;

				// Store the location of our simulated nails
				simulatedNails = new PointF[nailCount];

				// Minimum nail separation providing physical space for thread
				int minNailClearance = Math.Min(2, nailCount);

				// Space our nails evenly around a circular pattern
				double nailAngle = (Math.PI * 2.0) / (double)nailCount;
				nails = new Nail[nailCount];

				for (int nailIndex = 0; nailIndex < nailCount; ++nailIndex)
				{
					double nailX = 0.0;
					double nailY = 0.0;

					double currentAngle = nailAngle * (double)nailIndex;

					if (currentAngle < (Math.PI * 0.5))
					{
						nailX = Math.Sin(currentAngle);
						nailY = Math.Cos(currentAngle);
					}
					else if (currentAngle < Math.PI)
					{
						double localAngle = currentAngle - (Math.PI / 2.0);
						nailX = Math.Cos(localAngle);
						nailY = -1.0 * Math.Sin(localAngle);
					}
					else if (currentAngle < (Math.PI * 1.5))
					{
						double localAngle = currentAngle - Math.PI;
						nailX = -1.0 * Math.Sin(localAngle);
						nailY = -1.0 * Math.Cos(localAngle);
					}
					else
					{
						double localAngle = currentAngle - (Math.PI * 1.5);
						nailX = -1.0 * Math.Cos(localAngle);
						nailY = Math.Sin(localAngle);
					}

					PointF nailLocation = new PointF(nailRingRadius + ((float)nailX * nailRingRadius), nailRingRadius + ((float)nailY * nailRingRadius));

					// Determine which pixel this nail is located within
					Point nailPixel = GetPixelAtLocation(nailLocation);

					nails[nailIndex] = new Nail(nailPixel, nailLocation);

					simulatedNails[nailIndex] = new PointF(imageCenter + (float)(nailX * ringRadius), imageCenter + (float)(nailY * ringRadius));
				}

				if (simulatedNailRadius > zeroThreshold && ringRadius > zeroThreshold)
				{
					// Estimate an exit angle providing separation from the neighboring nail for thread
					double angleToTest = 0.0;
					PointF toNextNail = new PointF();
					toNextNail.X = Math.Abs(simulatedNails[1].X - simulatedNails[0].X);
					toNextNail.Y = Math.Abs(simulatedNails[1].Y - simulatedNails[0].Y) + (simulatedNailRadius * 2.0f);

					if (toNextNail.X > zeroThreshold)
					{
						angleToTest = Math.Atan(toNextNail.Y / toNextNail.X);
					}

					// Calculate a safe distance from the nail's center for thread to run
					double intersectDistance = (double)(simulatedNailRadius + threadDiameter);
					intersectDistance *= intersectDistance;

					for (int nailIndex = minNailClearance; nailIndex < nailCount; ++nailIndex)
					{
						PointF tangentOffset = CalculateNailTangentOffset(simulatedNailRadius, simulatedNails[nailIndex], simulatedNails[0]);
						PointF threadStart = new PointF(simulatedNails[nailIndex].X + tangentOffset.X, simulatedNails[nailIndex].Y + tangentOffset.Y);
						PointF threadEnd = new PointF(simulatedNails[0].X + tangentOffset.X, simulatedNails[0].Y + tangentOffset.Y);
						toNextNail.X = Math.Abs(threadStart.X - threadEnd.X);
						toNextNail.Y = Math.Abs(threadStart.Y - threadEnd.Y);

						if (toNextNail.X > zeroThreshold)
						{
							if (Math.Atan(toNextNail.Y / toNextNail.X) > angleToTest)
							{
								double clearanceDist = FindDistanceToSegmentSquared(simulatedNails[1], threadStart, threadEnd);
								if (clearanceDist > intersectDistance)
								{
									minNailClearance = nailIndex;
									break;
								}
							}
						}
					}
				}

				// Initialize our nails as able to connect with all others providing enough thread clearance
				nailAvailability = new bool[nailCount][];

				for (int nailIndex = 0; nailIndex < nailAvailability.Length; ++nailIndex)
				{
					nailAvailability[nailIndex] = new bool[nailCount];

					for (int connectionIndex = 0; connectionIndex < nailAvailability[nailIndex].Length; ++connectionIndex)
					{
						// Following our assumption that thread is always wrapped around nails in a
						// clockwise direction, only nails placed before this one need to be blocked.
						int nailDelta = nailIndex - connectionIndex;
						if (nailDelta < 0)
						{
							nailDelta += nailCount;
						}
						bool hasClearance = nailDelta > minNailClearance;
						nailAvailability[nailIndex][connectionIndex] = hasClearance;
					}
				}

				// Create an image we can periodically update to show progress to our user
				accessingPartialPreviewImage.WaitOne();

				if (null != partialPreviewImage)
				{
					partialPreviewImage.Dispose();
				}
				partialPreviewImage = new Bitmap(imageSize, imageSize);

				// Draw a background
				using (Graphics simGraphics = Graphics.FromImage(partialPreviewImage))
				{
					// Color in our background to help differentiate it from our simulated frame
					Brush backgroundBrush = new SolidBrush(previewBackgroundColor);
					simGraphics.FillRectangle(backgroundBrush, 0, 0, imageSize, imageSize);

					// Draw a white circle as our image background
					Brush circleBrush = new SolidBrush(previewCircleColor);
					simGraphics.FillEllipse(circleBrush, imageCenter - ringRadius, imageCenter - ringRadius, ringDiameter, ringDiameter);

					// Show all the nails
					if (simulatedNailRadius > zeroThreshold)
					{
						Brush nailBrush = new SolidBrush(Color.Gray);

						for (int nailIndex = 0; nailIndex < simulatedNails.Length; ++nailIndex)
						{
							simGraphics.FillEllipse(nailBrush,
													simulatedNails[nailIndex].X - simulatedNailRadius,
													simulatedNails[nailIndex].Y - simulatedNailRadius,
													2.0f * simulatedNailRadius,
													2.0f * simulatedNailRadius);
						}
					}
				}
				accessingPartialPreviewImage.ReleaseMutex();
			}
			else
			{
				MessageBox.Show("Please load a source image file.", "Source Image", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}

		private Task<int> CreateArtworkAsync()
		{
			return Task.Run(() =>
			{
				int lineCount = 0;

				if (sourceImage != null)
				{
					float artDiameter = nailRingDiameter - (artBorder * 2.0f);
					int borderPixels = (int)Math.Ceiling(artBorder / pixelSize);

					double halfPixel = pixelSize / 2.0;
					double pixelRadiusSquared = 2.0 * (halfPixel * halfPixel);
					double pixelRadius = Math.Sqrt(pixelRadiusSquared);
					float nailRadius = nailDiameter / 2.0f;

					bool useOldCode = false;

					double colorStep = 8.0;
					if (threadDensity > 0)
					{
						colorStep = 256.0 / (double)threadDensity;
					}

					// Store the delta values for all our potential lines
					int[] averageDeltas = new int[nailCount];
					int[] totalDeltas = new int[nailCount];

					// Would varying the starting nail would offer any benefits?
					int startNail = 0;

					// Prepare to store our ultimate goal, the line end points
					linePoints = new List<int>();
					linePoints.Add(startNail);

					accessingLinePoints.WaitOne();
					partialLinePoints = new List<int>();
					partialLinePoints.Add(startNail);
					accessingLinePoints.ReleaseMutex();

					for (; lineCount < maxLineCount; ++lineCount)
					{
						// Storing indexes for all pixels which would be effected by each line.
						// This avoids duplicating the traversal logic for our selected line.
						List<Point>[] effectedPixels = new List<Point>[nailCount];

						for (int endNail = 0; endNail < nailCount; ++endNail)
						{
							// Default our delta values to zero
							averageDeltas[endNail] = 0;
							totalDeltas[endNail] = 0;

							effectedPixels[endNail] = new List<Point>();

							// Skip any lines we already covered during an earlier iteration
							if (nailAvailability[startNail][endNail])
							{
								PointF startLocation = nails[startNail].Location;
								PointF endLocation = nails[endNail].Location;

								// Tangent intersection will be the same offset for both nails assuming
								// thread is always wrapped in a clockwise direction.
								PointF tangentOffset = CalculateNailTangentOffset(nailRadius, startLocation, endLocation);
								startLocation.X += tangentOffset.X;
								startLocation.Y += tangentOffset.Y;
								endLocation.X += tangentOffset.X;
								endLocation.Y += tangentOffset.Y;

								Point startPixel = GetPixelAtLocation(startLocation);
								Point endPixel = GetPixelAtLocation(endLocation);

								int deltaX = endPixel.X - startPixel.X;
								int deltaY = endPixel.Y - startPixel.Y;

								// Iterate along the longest axis so each step can be a single pixel.
								// A lot of very similar code in both branches, but consolidating it
								// did not seem like it would be worth the time.
								if (Math.Abs(deltaX) > Math.Abs(deltaY))
								{
									// Setup some local variables to ensure our increments are
									// increasing along X to simplify the logic a bit.
									Point currentPixel;
									Point targetPixel;
									if (startPixel.X < endPixel.X)
									{
										currentPixel = startPixel;
										targetPixel = endPixel;
									}
									else
									{
										currentPixel = endPixel;
										targetPixel = startPixel;
									}

									float accumulatedY = 0.0f;
									float slope = Math.Abs((float)deltaY / (float)deltaX);
									if (currentPixel.Y > targetPixel.Y)
									{
										slope *= -1.0f;
									}

									while (currentPixel.X < targetPixel.X)
									{
										// Test neighboring pixels as long as they are also within range
										int pixelOffset = -1 * lineOffsetCheck;
										while ((currentPixel.Y + pixelOffset) < 0)
										{
											//Debug.WriteLine("Seems the offset is pushing Y out of range. Index[" + currentPixel.Y + "] Offset[" + pixelOffset + "]");
											++pixelOffset;
										}
										int maxOffset = lineOffsetCheck + 1;
										while ((currentPixel.Y + maxOffset) > totalPixels)
										{
											//Debug.WriteLine("Seems the offset is pushing Y out of range. Index[" + currentPixel.Y + "] Offset[" + pixelOffset + "]");
											--maxOffset;
										}

										// Now we can safely perform the actual pixel tests
										for (; pixelOffset < maxOffset; ++pixelOffset)
										{
											int currentY = currentPixel.Y + pixelOffset;
											if (artPixels[currentPixel.X, currentY].IsValid())
											{
												// Determine if our line will cover part of this pixel
												double lineDist = FindDistanceToSegmentSquared(artPixels[currentPixel.X, currentY].GetLocation(),
																								startLocation,
																								endLocation);
												if (lineDist < pixelRadiusSquared)
												{
													// Scale this pixel's potential value change based on the thread's distance from center.
													// Normalized distance is squared to further favor intersections nearer the pixel's center,
													// though utilizing another curve (ex: cosine) might be even better.
													double normalizedDist = 1.0 - (Math.Sqrt(lineDist) / pixelRadius);
													totalDeltas[endNail] += artPixels[currentPixel.X, currentY].CalculateChange((int)(normalizedDist * normalizedDist * colorStep));

													effectedPixels[endNail].Add(new Point(currentPixel.X, currentY));
												}
											}
										}

										// Move our X value the distance of one pixel's width
										currentPixel.X += 1;

										// Our new Y value is dependent on this line's slope
										accumulatedY += slope;
										if (accumulatedY >= 1.0f)
										{
											accumulatedY -= 1.0f;
											currentPixel.Y += 1;
										}
										else if (accumulatedY <= -1.0f)
										{
											accumulatedY += 1.0f;

											if (currentPixel.Y > 0)
											{
												currentPixel.Y -= 1;
											}
											else
											{
												currentPixel.Y = 0;
											}
										}
									}
								}
								else
								{
									// Setup some local variables to ensure our increments are
									// increasing along Y to simplify the logic a bit.
									Point currentPixel;
									Point targetPixel;
									if (startPixel.Y < endPixel.Y)
									{
										currentPixel = startPixel;
										targetPixel = endPixel;
									}
									else
									{
										currentPixel = endPixel;
										targetPixel = startPixel;
									}

									float accumulatedX = 0.0f;
									float slope = Math.Abs((float)deltaX / (float)deltaY);
									if (currentPixel.X > targetPixel.X)
									{
										slope *= -1.0f;
									}

									while (currentPixel.Y < targetPixel.Y)
									{
										// Test neighboring pixels as long as they are also within range
										int pixelOffset = -1 * lineOffsetCheck;
										while ((currentPixel.X + pixelOffset) < 0)
										{
											//Debug.WriteLine("Seems the offset is pushing X out of range. Index[" + currentPixel.X + "] Offset[" + pixelOffset + "]");
											++pixelOffset;
										}
										int maxOffset = lineOffsetCheck + 1;
										while ((currentPixel.X + maxOffset) > totalPixels)
										{
											//Debug.WriteLine("Seems the offset is pushing X out of range. Index[" + currentPixel.X + "] Offset[" + pixelOffset + "]");
											--maxOffset;
										}

										// Now we can safely perform the actual pixel tests
										for (; pixelOffset < maxOffset; ++pixelOffset)
										{
											int currentX = currentPixel.X + pixelOffset;
											if (artPixels[currentX, currentPixel.Y].IsValid())
											{
												// Determine if our line will cover part of this pixel
												double lineDist = FindDistanceToSegmentSquared(artPixels[currentX, currentPixel.Y].GetLocation(),
																								startLocation,
																								endLocation);
												if (lineDist < pixelRadiusSquared)
												{
													// Scale this pixel's potential value change based on the thread's distance from center.
													// Normalized distance is squared to further favor intersections nearer the pixel's center,
													// though utilizing another curve (ex: cosine) might be even better.
													double normalizedDist = 1.0 - (Math.Sqrt(lineDist) / pixelRadius);
													totalDeltas[endNail] += artPixels[currentX, currentPixel.Y].CalculateChange((int)(normalizedDist * normalizedDist * colorStep));

													effectedPixels[endNail].Add(new Point(currentX, currentPixel.Y));
												}
											}
										}

										// Move our Y value the distance of one pixel's width
										currentPixel.Y += 1;

										// Our new X value is dependent on this line's slope
										accumulatedX += slope;
										if (accumulatedX >= 1.0f)
										{
											accumulatedX -= 1.0f;
											currentPixel.X += 1;
										}
										else if (accumulatedX <= -1.0f)
										{
											accumulatedX += 1.0f;

											if (currentPixel.X > 0)
											{
												currentPixel.X -= 1;
											}
											else
											{
												currentPixel.X = 0;
											}
										}
									}
								}

								if (effectedPixels[endNail].Count > 0)
								{
									averageDeltas[endNail] = totalDeltas[endNail] / effectedPixels[endNail].Count;
								}
							}
						}

						int nextNail = -1;
						int largestDelta = 0;
						int currentTotalDelta = 0;

						//for (int x = 0; x < nailCount; ++x)
						//{
						//	if (totalDeltas[x] > largestDelta)
						//	{
						//		nextNail = x;
						//		largestDelta = totalDeltas[x];
						//	}
						//}

						for (int x = 0; x < nailCount; ++x)
						{
							if (averageDeltas[x] > largestDelta)
							{
								nextNail = x;
								largestDelta = averageDeltas[x];
							}
							else if (averageDeltas[x] == largestDelta)
							{
								if (totalDeltas[x] > currentTotalDelta)
								{
									currentTotalDelta = totalDeltas[x];
									nextNail = x;
								}
							}
						}

						if (nextNail > -1)
						{
							if (useOldCode)
							{
								foreach (Point pixel in effectedPixels[nextNail])
								{
									if (artPixels[pixel.X, pixel.Y].IsValid())
									{
										// Determine if our line will cover part of this pixel
										double lineDist = FindDistanceToSegmentSquared(artPixels[pixel.X, pixel.Y].GetLocation(),
																						nails[startNail].Location,
																						nails[nextNail].Location);
										if (lineDist < pixelRadiusSquared)
										{
											// Scale this pixel's potential value change based on the thread's distance from center
											double normalizedDist = Math.Sqrt(lineDist) / pixelRadius;
											artPixels[pixel.X, pixel.Y].ApplyChange((int)(normalizedDist * colorStep));
										}
									}
								}
							}
							else
							{
								PointF startLocation = nails[startNail].Location;
								PointF endLocation = nails[nextNail].Location;

								// Tangent intersection will be the same offset for both nails assuming
								// thread is always wrapped in a clockwise direction.
								PointF tangentOffset = CalculateNailTangentOffset(nailRadius, startLocation, endLocation);
								startLocation.X += tangentOffset.X;
								startLocation.Y += tangentOffset.Y;
								endLocation.X += tangentOffset.X;
								endLocation.Y += tangentOffset.Y;

								foreach (Point pixel in effectedPixels[nextNail])
								{
									if (artPixels[pixel.X, pixel.Y].IsValid())
									{
										// Determine if our line will cover part of this pixel
										double lineDist = FindDistanceToSegmentSquared(artPixels[pixel.X, pixel.Y].GetLocation(),
																						startLocation,
																						endLocation);
										if (lineDist < pixelRadiusSquared)
										{
											// Scale this pixel's potential value change based on the thread's distance from center
											double normalizedDist = Math.Sqrt(lineDist) / pixelRadius;
											artPixels[pixel.X, pixel.Y].ApplyChange((int)(normalizedDist * colorStep));
										}
									}
								}
							}

							nailAvailability[startNail][nextNail] = false;

							if (nailDiameter < zeroThreshold)
							{
								// Avoid duplicate lines
								nailAvailability[nextNail][startNail] = false;
							}

							startNail = nextNail;
							linePoints.Add(nextNail);
							
							// Include this nail in the queue for our next drawing update
							accessingLinePoints.WaitOne();
							partialLinePoints.Add(nextNail);
							accessingLinePoints.ReleaseMutex();
							
						}
						else
						{
							break;
						}
					}
				}

				return lineCount;
			});
		}

		private void GenerateSimulatedImage()
		{
			// Calculate how large an image is required for 1 pixel to equal our thread's diameter
			float threadsPerPixel = pixelSize / threadDiameter;
			float ringDiameter = threadsPerPixel * (float)totalPixels;
			float ringRadius = ringDiameter / 2.0f;
			float imageCenter = (ringDiameter + (nailDiameter * threadsPerPixel)) / 2.0f;
			int imageSize = (int)Math.Round(imageCenter * 2.0f);
			float nailRadius = nailDiameter * threadsPerPixel / 2.0f;

			if (null != previewImage)
			{
				previewImage.Dispose();
				previewImage = null;
			}

			// Create a new blank image based on our calculated dimensions
			Bitmap simulatedImage = new Bitmap(imageSize, imageSize);
			using (Graphics simGraphics = Graphics.FromImage(simulatedImage))
			{
				Pen stringPen = new Pen(previewLineColor, 1.0f);

				// Color in our background to help differentiate it from our simulated frame
				Brush backgroundBrush = new SolidBrush(previewBackgroundColor);
				simGraphics.FillRectangle(backgroundBrush, 0, 0, imageSize, imageSize);

				// Draw a white circle as our image background
				Brush circleBrush = new SolidBrush(previewCircleColor);
				simGraphics.FillEllipse(circleBrush, imageCenter - ringRadius, imageCenter - ringRadius, ringDiameter, ringDiameter);

				if (linePoints != null)
				{
					if (linePoints.Count > 0)
					{
						// Show all the nails
						if (nailRadius > zeroThreshold)
						{
							Brush nailBrush = new SolidBrush(Color.Gray);
							for (int nailIndex = 0; nailIndex < simulatedNails.Length; ++nailIndex)
							{
								simGraphics.FillEllipse(nailBrush,
														simulatedNails[nailIndex].X - nailRadius,
														simulatedNails[nailIndex].Y - nailRadius,
														2.0f * nailRadius,
														2.0f * nailRadius);
							}
						}

						// Finally we can actually draw all the lines!
						for (int nailIndex = 1; nailIndex < linePoints.Count; ++nailIndex)
						{
							PointF startLocation = simulatedNails[linePoints[nailIndex - 1]];
							PointF endLocation = simulatedNails[linePoints[nailIndex]];

							// Tangent intersection will be the same offset for both nails assuming
							// thread is always wrapped in a clockwise direction.
							PointF tangentOffset = CalculateNailTangentOffset(nailRadius, startLocation, endLocation);
							startLocation.X += tangentOffset.X;
							startLocation.Y += tangentOffset.Y;
							endLocation.X += tangentOffset.X;
							endLocation.Y += tangentOffset.Y;

							//// Draw the tangent offsets for visual debugging
							//Pen startPen = new Pen(Color.Green, 1.0f);
							//simGraphics.DrawLine(startPen, simulatedNails[linePoints[nailIndex - 1]], startLocation);

							//Pen endPen = new Pen(Color.Red, 1.0f);
							//simGraphics.DrawLine(endPen, simulatedNails[linePoints[nailIndex]], endLocation);

							simGraphics.DrawLine(stringPen, startLocation, endLocation);
						}
					}
				}


				//// Creating an illustrative example for our documentation
				//if (null != simulatedNails)
				//{
				//	Brush nailBrush = new SolidBrush(Color.Gray);
				//	simGraphics.FillEllipse(nailBrush, simulatedNails[0].X - nailRadius, simulatedNails[0].Y - nailRadius, 2.0f * nailRadius, 2.0f * nailRadius);

				//	for (int nailIndex = 1; nailIndex < simulatedNails.Length; ++nailIndex)
				//	{
				//		simGraphics.FillEllipse(nailBrush, simulatedNails[nailIndex].X - nailRadius, simulatedNails[nailIndex].Y - nailRadius, 2.0f * nailRadius, 2.0f * nailRadius);
				//		simGraphics.DrawLine(stringPen, simulatedNails[0], simulatedNails[nailIndex]);
				//	}
				//}

				int preivewSize = Math.Min(simulatedImage.Width, 1200);

				previewImage = new Bitmap(preivewSize, preivewSize);
				using (Graphics previewGraphics = Graphics.FromImage(previewImage))
				{
					previewGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
					previewGraphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
					previewGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
					previewGraphics.DrawImage(simulatedImage, 0, 0, preivewSize, preivewSize);
				}
			}
		}

		// This logic is definitely a bit cumbersome, but ultimately produced more reliable/safer
		// results than attempts to dynamically calculate pixel indexes based on their location.
		private Point GetPixelAtLocation(PointF location)
		{
			Point nailPixel = new Point(-1, -1);

			// Slight differences in floating point precision resulted in comparisons of what
			// should be identical values failing an equality test.  Switching to a check of
			// positional values being within our desired range plus a small value worked better.
			float maxDelta = zeroThreshold + (pixelSize / 2.0f);

			for (int x = 0; x < totalPixels; ++x)
			{
				PointF pixelLocation = artPixels[x, 0].GetLocation();

				// Check if we are in the correct column of pixels
				if (Math.Abs(location.X - pixelLocation.X) < maxDelta)
				{
					nailPixel.X = x;

					// Yup, so now we just need to find the intersecting row of pixels
					for (int y = 0; y < totalPixels; ++y)
					{
						pixelLocation = artPixels[x, y].GetLocation();

						if (Math.Abs(location.Y - pixelLocation.Y) < maxDelta)
						{
							nailPixel.Y = y;
							break;
						}
					}

					if (-1 == nailPixel.Y)
					{
						// Occasionally a nail's position is outside all of our pixels due
						// to floating point imprecision.  This occurs at an edge case along
						// the outermost reach of our arrays, so default to our final index.
						nailPixel.Y = totalPixels - 1;
					}
					break;
				}
			}

			if (-1 == nailPixel.X)
			{
				// Occasionally a nail's position is outside all of our pixels due
				// to floating point imprecision.  This occurs at an edge case along
				// the outermost reach of our arrays, so default to our final index.
				nailPixel.X = totalPixels - 1;

				// Still need to find the intersecting row of pixels
				for (int y = 0; y < totalPixels; ++y)
				{
					PointF pixelLocation = artPixels[nailPixel.X, y].GetLocation();

					if (Math.Abs(location.Y - pixelLocation.Y) < maxDelta)
					{
						nailPixel.Y = y;
						break;
					}
				}

				if (-1 == nailPixel.Y)
				{
					// Occasionally a nail's position is outside all of our pixels due
					// to floating point imprecision.  This occurs at an edge case along
					// the outermost reach of our arrays, so default to our final index.
					nailPixel.Y = totalPixels - 1;
				}
			}

			return nailPixel;
		}

		// Calculate the distance between point pt and the segment p1 --> p2.
		private double FindDistanceToSegment(PointF pt, PointF p1, PointF p2)
		{
			float dx = p2.X - p1.X;
			float dy = p2.Y - p1.Y;
			if ((dx == 0) && (dy == 0))
			{
				// It's a point not a line segment.
				dx = pt.X - p1.X;
				dy = pt.Y - p1.Y;
				return Math.Sqrt(dx * dx + dy * dy);
			}

			// Calculate the t that minimizes the distance.
			float t = ((pt.X - p1.X) * dx + (pt.Y - p1.Y) * dy) / (dx * dx + dy * dy);

			// See if this represents one of the segment's
			// end points or a point in the middle.
			if (t < 0)
			{
				dx = pt.X - p1.X;
				dy = pt.Y - p1.Y;
			}
			else if (t > 1)
			{
				dx = pt.X - p2.X;
				dy = pt.Y - p2.Y;
			}
			else
			{
				PointF closest = new PointF(p1.X + t * dx, p1.Y + t * dy);
				dx = pt.X - closest.X;
				dy = pt.Y - closest.Y;
			}

			return Math.Sqrt(dx * dx + dy * dy);
		}

		// Calculate the distance between point pt and the segment p1 --> p2.
		private double FindDistanceToSegmentSquared(PointF pt, PointF p1, PointF p2)
		{
			float dx = p2.X - p1.X;
			float dy = p2.Y - p1.Y;
			if ((dx == 0) && (dy == 0))
			{
				// It's a point not a line segment.
				dx = pt.X - p1.X;
				dy = pt.Y - p1.Y;
				return (dx * dx) + (dy * dy);
			}

			// Calculate the t that minimizes the distance.
			float t = ((pt.X - p1.X) * dx + (pt.Y - p1.Y) * dy) / (dx * dx + dy * dy);

			// See if this represents one of the segment's
			// end points or a point in the middle.
			if (t < 0)
			{
				dx = pt.X - p1.X;
				dy = pt.Y - p1.Y;
			}
			else if (t > 1)
			{
				dx = pt.X - p2.X;
				dy = pt.Y - p2.Y;
			}
			else
			{
				dx = pt.X - (p1.X + t * dx);
				dy = pt.Y - (p1.Y + t * dy);
			}

			return (dx * dx) + (dy * dy);
		}

		private void EnableUI(bool isEnabled)
		{
			cbNailCount.Enabled = isEnabled;
			tbDiameter.Enabled = isEnabled;
			tbBorder.Enabled = isEnabled;
			tbMaxLines.Enabled = isEnabled;
			tbImagePath.Enabled = isEnabled;
			btnOpenImage.Enabled = isEnabled;
			tbNailDiameter.Enabled = isEnabled;
			tbThreadWidth.Enabled = isEnabled;
			tbThreadDensity.Enabled = isEnabled;

			if (isEnabled)
			{
				btnSimulate.Enabled = null != sourceImage;
				btnSavePreview.Enabled = null != sourceImage;
				btnSavePattern.Enabled = linePoints.Count > 0;
			}
			else
			{
				btnSimulate.Enabled = false;
				btnSavePreview.Enabled = false;
				btnSavePattern.Enabled = false;
			}
		}

		private int CalculateThreadLength()
		{
			double threadLength = 0.0;

			double nailWrap = 0.0f;
			if (nailDiameter > zeroThreshold)
			{
				// Estimating thread will be wrapped around half of each nail on average
				nailWrap = nailDiameter * Math.PI / 2.0;
			}

			for (int x = 1; x < linePoints.Count; ++x)
			{
				PointF startNail = nails[linePoints[x - 1]].Location;
				PointF endNail = nails[linePoints[x]].Location;
				SizeF thread = new SizeF(endNail.X - startNail.X, endNail.Y - startNail.Y);
				threadLength += Math.Sqrt((thread.Width * thread.Width) + (thread.Height * thread.Height));
				threadLength += nailWrap;
			}

			return (int)Math.Round(threadLength / 1000.0);
		}

		private PointF CalculateNailTangentOffset(float nailRadius, PointF startLocation, PointF endLocation)
		{
			PointF tangentOffset = new PointF(0.0f, 0.0f);

			if (nailRadius > zeroThreshold)
			{
				// Start by finding the deltas for each axis
				float deltaX = endLocation.X - startLocation.X;
				float deltaY = endLocation.Y - startLocation.Y;
				float vectorLength = (float)Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY));

				if (vectorLength > zeroThreshold)
				{
					// Transform the values into normalized vector components
					deltaX /= vectorLength;
					deltaY /= vectorLength;

					// Use those normalized values to calculate the offset based on our nail diameter
					// while switching the resulting vector's direction to be perpendicular.
					tangentOffset.X = nailRadius * deltaY;
					tangentOffset.Y = -1.0f * nailRadius * deltaX;
				}
			}

			return tangentOffset;
		}

		private void UpdateUserParameters()
		{
			// Update our parameters based on the user's most recent data
			int tempInt = 0;
			float tempFloat = 0.0f;

			// Nail count
			if (float.TryParse(cbNailCount.Text, out tempFloat))
			{
				tempInt = (int)Math.Round(tempFloat);
				if (tempInt > 0)
				{
					nailCount = tempInt;
				}
			}

			// Nail diameter
			if (float.TryParse(tbNailDiameter.Text, out tempFloat))
			{
				if (tempFloat > 0.1f)
				{
					nailDiameter = tempFloat;
				}
			}

			// Nail ring diameter
			if (float.TryParse(tbDiameter.Text, out tempFloat))
			{
				if (tempFloat >= 1.0f)
				{
					nailRingDiameter = tempFloat;
				}
			}

			// Border width
			if (float.TryParse(tbBorder.Text, out tempFloat))
			{
				if (tempFloat > 0.0f && tempFloat < (2.0f * nailRingDiameter))
				{
					nailRingDiameter = tempFloat;
				}
			}

			// Maximum number of lines
			if (float.TryParse(tbMaxLines.Text, out tempFloat))
			{
				tempInt = (int)Math.Round(tempFloat);
				if (tempInt > 0)
				{
					maxLineCount = tempInt;
				}
			}

			// Thread diameter
			if (float.TryParse(tbThreadWidth.Text, out tempFloat))
			{
				tempInt = (int)Math.Round(tempFloat);
				if (tempInt > zeroThreshold)
				{
					threadDiameter = tempInt;
				}
			}

			// Pixel thread density
			if (float.TryParse(tbThreadDensity.Text, out tempFloat))
			{
				tempInt = (int)Math.Round(tempFloat);
				if (tempInt > 0 && tempInt < 256)
				{
					threadDensity = tempInt;
				}
			}
		}

		private void btnSavePattern_Click(object sender, EventArgs e)
		{
			SaveFileDialog dlgSavePattern = new SaveFileDialog();

			dlgSavePattern.Filter = "txt files (*.txt)|*.txt";
			dlgSavePattern.FilterIndex = 1;
			dlgSavePattern.RestoreDirectory = true;
			dlgSavePattern.FileName = lineFilePath;

			if (DialogResult.OK == dlgSavePattern.ShowDialog())
			{
				Stream myStream = dlgSavePattern.OpenFile();
				if (null != myStream)
				{
					using (StreamWriter writer = new StreamWriter(myStream))
					{
						int linePointIndex = 0;
						while (linePointIndex < linePoints.Count)
						{
							string lineInfo = "";
							for (int columnIndex = 0; columnIndex < 16 && linePointIndex < linePoints.Count; ++columnIndex, ++linePointIndex)
							{
								// Most users are probably not used to zero based arrays, so offset by one
								lineInfo += string.Format("{0,3}, ", linePoints[linePointIndex] + 1);
							}
							writer.WriteLine(lineInfo);
						}
					}

					myStream.Close();
				}
			}
		}

		private void tbNailDiameter_Validating(object sender, CancelEventArgs e)
		{
			float updatedDiameter = nailDiameter;
			if (float.TryParse(tbNailDiameter.Text, out updatedDiameter))
			{
				if (updatedDiameter > zeroThreshold)
				{
					nailDiameter = updatedDiameter;
				}
				else
				{
					nailDiameter = 0.0f;
					tbNailDiameter.Text = "0";
				}
			}
			else
			{
				tbNailDiameter.Text = nailDiameter.ToString();
				e.Cancel = true;
			}
		}

		private void tbDiameter_Validating(object sender, CancelEventArgs e)
		{
			float updatedDiameter = nailRingDiameter;
			if (float.TryParse(tbDiameter.Text, out updatedDiameter))
			{
				if (updatedDiameter >= 1.0)
				{
					nailRingDiameter = updatedDiameter;
				}
				else
				{
					tbDiameter.Text = nailRingDiameter.ToString();
					e.Cancel = true;
				}
			}
			else
			{
				tbDiameter.Text = nailRingDiameter.ToString();
				e.Cancel = true;
			}
		}

		private void tbBorder_Validating(object sender, CancelEventArgs e)
		{
			float updatedBorder = artBorder;
			if (float.TryParse(tbBorder.Text, out updatedBorder))
			{
				if (updatedBorder < zeroThreshold || nailRingDiameter < (2.0f * updatedBorder))
				{
					tbBorder.Text = "0";
					artBorder = 0.0f;
				}
				else
				{
					artBorder = updatedBorder;
				}
			}
			else
			{
				tbBorder.Text = artBorder.ToString();
				e.Cancel = true;
			}
		}

		private void tbMaxLines_Validating(object sender, CancelEventArgs e)
		{
			int updatedMax = 0;
			float tempFloat = 0.0f;
			if (float.TryParse(tbMaxLines.Text, out tempFloat))
			{
				updatedMax = (int)Math.Round(tempFloat);
			}

			if (updatedMax > 0)
			{
				maxLineCount = updatedMax;
			}
			else
			{
				tbMaxLines.Text = maxLineCount.ToString();
				e.Cancel = true;
			}
		}

		private void tbThreadWidth_Validating(object sender, CancelEventArgs e)
		{
			float updatedWidth = 0.0f;
			if (float.TryParse(tbThreadWidth.Text, out updatedWidth))
			{
				if (updatedWidth >= zeroThreshold)
				{
					threadDiameter = updatedWidth;
				}
				else
				{
					tbThreadWidth.Text = threadDiameter.ToString();
					e.Cancel = true;
				}
			}
			else
			{
				tbThreadWidth.Text = threadDiameter.ToString();
				e.Cancel = true;
			}
		}

		private void tbThreadDensity_Validating(object sender, CancelEventArgs e)
		{
			float tempFloat = 0.0f;
			if (float.TryParse(tbThreadDensity.Text, out tempFloat))
			{
				int updatedDensity = (int)Math.Round(tempFloat);
				if (updatedDensity < 1)
				{
					threadDensity = 1;
					tbThreadDensity.Text = threadDensity.ToString();
					e.Cancel = true;
				}
				else if (updatedDensity > 255)
				{
					threadDensity = 255;
					tbThreadDensity.Text = threadDensity.ToString();
					e.Cancel = true;
				}
				else
				{
					threadDensity = updatedDensity;
				}
			}
			else
			{
				tbThreadDensity.Text = threadDensity.ToString();
				e.Cancel = true;
			}
		}

		private void btnSavePreview_Click(object sender, EventArgs e)
		{
			if (null != previewImage)
			{
				SaveFileDialog dlgSavePattern = new SaveFileDialog();

				dlgSavePattern.Filter = "JPEG|*.jpg;*.jpeg|BMP|*.bmp|GIF|*.gif|EXIF|*.exif|TIFF|*.tiff|PNG|*.png";
				dlgSavePattern.FilterIndex = 1;
				dlgSavePattern.RestoreDirectory = true;
				dlgSavePattern.FileName = previewImagePath;

				if (System.Drawing.Imaging.ImageFormat.Bmp == previewImageFormat)
				{
					dlgSavePattern.FilterIndex = 2;
				}
				else if (System.Drawing.Imaging.ImageFormat.Gif == previewImageFormat)
				{
					dlgSavePattern.FilterIndex = 3;
				}
				else if (System.Drawing.Imaging.ImageFormat.Exif == previewImageFormat)
				{
					dlgSavePattern.FilterIndex = 4;
				}
				else if (System.Drawing.Imaging.ImageFormat.Tiff == previewImageFormat)
				{
					dlgSavePattern.FilterIndex = 5;
				}
				else if (System.Drawing.Imaging.ImageFormat.Png == previewImageFormat)
				{
					dlgSavePattern.FilterIndex = 6;
				}

				if (DialogResult.OK == dlgSavePattern.ShowDialog())
				{
					switch (dlgSavePattern.FilterIndex)
					{
						case 2:
							previewImageFormat = System.Drawing.Imaging.ImageFormat.Bmp;
							break;

						case 3:
							previewImageFormat = System.Drawing.Imaging.ImageFormat.Gif;
							break;

						case 4:
							previewImageFormat = System.Drawing.Imaging.ImageFormat.Exif;
							break;

						case 5:
							previewImageFormat = System.Drawing.Imaging.ImageFormat.Tiff;
							break;

						case 6:
							previewImageFormat = System.Drawing.Imaging.ImageFormat.Png;
							break;

						default:
							previewImageFormat = System.Drawing.Imaging.ImageFormat.Jpeg;
							break;
					}

					previewImagePath = dlgSavePattern.FileName;
					previewImage.Save(previewImagePath, previewImageFormat);
				}
			}
		}

		private void OnDrawPartialPreviewTimer(Object source, EventArgs e)
		{
			Debug.WriteLine("OnDrawPartialPreviewTimer()");
			// Is this the best way to present in-progress data visualization for our
			// user?  No idea.  Functions acceptably so keeping it as is for now.
			if (null != partialPreviewImage)
			{
				int[] nailCache = null;

				accessingLinePoints.WaitOne();

				if (partialLinePoints.Count > 1)
				{
					nailCache = new int[partialLinePoints.Count];
					partialLinePoints.CopyTo(nailCache);
					partialLinePoints.Clear();
					partialLinePoints.Add(nailCache[nailCache.Length - 1]);
				}

				accessingLinePoints.ReleaseMutex();

				if (null != nailCache)
				{
					accessingPartialPreviewImage.WaitOne();

					using (Graphics simGraphics = Graphics.FromImage(partialPreviewImage))
					{
						Pen stringPen = new Pen(previewLineColor, 1.0f);

						// Draw all the new lines onto our simulation image
						for (int nailIndex = 1; nailIndex < nailCache.Length; ++nailIndex)
						{
							PointF startLocation = simulatedNails[nailCache[nailIndex - 1]];
							PointF endLocation = simulatedNails[nailCache[nailIndex]];

							// Tangent intersection will be the same offset for both nails assuming
							// thread is always wrapped in a clockwise direction.
							PointF tangentOffset = CalculateNailTangentOffset(simulatedNailRadius, startLocation, endLocation);
							startLocation.X += tangentOffset.X;
							startLocation.Y += tangentOffset.Y;
							endLocation.X += tangentOffset.X;
							endLocation.Y += tangentOffset.Y;

							simGraphics.DrawLine(stringPen, startLocation, endLocation);
						}
					}

					using (Graphics panelGraphics = panelCanvas.CreateGraphics())
					{
						Point canvasCenter = new Point(panelCanvas.Width / 2, panelCanvas.Height / 2);
						int canvasWidth = Math.Min(panelCanvas.Height, panelCanvas.Width);

						// Scale the preview image to fit within our UI's panel
						panelGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
						panelGraphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
						panelGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
						panelGraphics.DrawImage(partialPreviewImage,
												canvasCenter.X - (canvasWidth / 2),
												canvasCenter.Y - (canvasWidth / 2),
												canvasWidth,
												canvasWidth);
					}

					accessingPartialPreviewImage.ReleaseMutex();
				}
			}

			if (null != partialPreviewDrawTimer)
			{
				partialPreviewDrawTimer.Enabled = true;
			}
		}
	}

	public class Pixel
	{
		int originalValue = 0;
		int currentValue = 0;
		bool visible = true;
		PointF location;

		public Pixel(PointF loc, bool isValid)
		{
			location = new PointF(loc.X, loc.Y);
			visible = isValid;
		}

		public void SetStartingColor(Color start)
		{
			// Calculating the average value just in case it is not truly gray-scale data
			originalValue = start.R;
			originalValue += start.G;
			originalValue += start.B;
			originalValue /= 3;

			// Using the inverse to make our later calculations a bit nicer
			originalValue = 255 - originalValue;

			// Trying an exponential curve to effectively increase contrast
			float scaler = (float)originalValue / 255.0f;
			originalValue = (int)(255.0f * scaler * scaler);

			currentValue = originalValue;
		}

		public int CalculateChange(int delta)
		{
			int change = 0;

			if (currentValue > 0)
			{
				if (currentValue > delta)
				{
					change = delta;
				}
				else
				{
					change = delta - (currentValue * 2);
				}
			}
			else
			{
				change = -1 * delta;
			}

			return change;
		}

		public void ApplyChange(int delta)
		{

			if (currentValue > 0)
			{
				if (currentValue > delta)
				{
					currentValue -= delta;
				}
				else
				{
					currentValue = delta - (currentValue * 2);
				}
			}
			else
			{
				currentValue -= delta;
			}
		}

		public bool IsValid() { return visible; }
		public int GetCurrentValue() { return currentValue; }
		public PointF GetLocation() { return location; }
	}

	public class Nail
	{
		public Nail(Point pixel, PointF loc)
		{
			PixelIndex = pixel;
			Location = loc;
		}

		public Point PixelIndex { get; set; }
		public PointF Location { get; set; }
	}

	public class ResultData
	{
		public int Id { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public double TotalSeconds { get; set; }
		public double TotalMinutes { get; set; }
		public double TotalHours { get; set; }
		public int LineCount { get; set; }
	}
}
