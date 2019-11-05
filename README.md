Scripts to generate synthetic training data from Unity game engine and plug into a modified RetinaNet implementation that finds the main wall in an image.

Steps:


1. Create a new Unity3D project
2. Import a photo-realistic room.  I used (this bedroom)[https://assetstore.unity.com/packages/3d/props/furniture/bedroom-architect-series-85476] back when it was free (sorry! It's $5 now, but there's other ones you can use).
3. Create a "training" directory containing "imgs" and "labels" folders.
4. Grab this script and potentially modify TODO
5. Attach the script to the Main Camera object.
6. Run the project to start generating images and coordinate labels.
7. Run TODO to aggregate all the generated labels into a single CSV file
8. Run the RetinaNet implementation modified to support quadrilaterals instead of just bounding boxes.  The command I used for training was:

`python3 keras_retinanet/bin/train.py --points=4 --image-min-side=571 csv "path/to/training/annotations.csv" "path/to/training/classes.csv"`
