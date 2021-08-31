# GishThreadArt
[GISH](https://www.gish.com/) 2021 asked participants to create a portrait of a Supernatural character out of a single thread, in the style of Slovenian artist Cvern.  I felt more confident applying my programming skills toward this task rather than relying on pure artistic ability.  As a result I spent a couple days during the hunt writing this app which calculates how to route thread between nails recreating a digital image.

![Screenshot of the app](/assets/Capture8.JPG)

Details about the app's internal logic are available on the [Team BetaBlue website](https://www.gishbetablue.com/thread-art).

## Image
Identify the digital image you want converted into thread art.

## Nails
### Count
Total number of nails placed around the frame's edge.  Currently limited to amounts based on easily dividing 360° evenly.

### Nail Diameter
Width in milimeters of the nails thread will be wrapped around.  Zero is considered valid in case the thread is being held in place by something other than a nail.  Inserting into a hole drilled through the frame for example.

### Ring Diameter
Total width in milimeters of the circle nails will be located around.  It passes through the center of all nails.

## Artwork
### Border
Width in milimeters of a ring around the perimeter of the nail circle containing no image data.  Theoretically creates a border around the thread art.  Did not seem super useful in practice, but leaving this functionality in case anyone wants to experiment with it.

### Max Lines
Processing will stop upon reaching this number of individual lengths of thread.  It may also complete earlier if all pixels get covered.

### Thread Width
Currently only used when creating the preview image as realistic as possible.  Value is in milimeters.

### Density
Defines how many threads must cover a pixel before it is considered completely black.  Actual logic is a bit more complicated, but that conveys the basic concept.  Adjusting this value can produce varying results from a minimistic reproduction to extremely dark shadows.
![Screenshot of the app](/assets/thread_density_examples_text_stack.jpg)

## Process Image
Begins the actual conversion of a source image into thread art.

## Save Preview
Allows saving a higher resolution copy of the simulated preview image.

## Save Pattern
Creates a text file listing nail ID numbers in the calculated order to reproduce the simulated image.  Nail \#1 is at the very bottom center of the frame.  ID numbers increase moving counterclockwise around the frame.
![Screenshot of the app](/assets/nail_layout.jpg)
