Scripts to generate synthetic training data from Unity game engine and plug into a modified RetinaNet implementation that finds the main wall in an image.

Full project summary is [here](https://letsmakeit.com/training-your-ai-in-the-matrix/).

Steps:


1. Create a new Unity3D project
2. Import a photo-realistic room.  I used [this bedroom](https://assetstore.unity.com/packages/3d/props/furniture/bedroom-architect-series-85476) back when it was free (sorry! It's $5 now, but there's other ones you can use).
3. Create a "training" directory containing "imgs" and "labels" folders.  You'll need to change other variables below if using a different room.
4. Grab Pics.cs and change the trainingDir variable to point to your "training" folder from above.
5. Attach the Pics.cs script to the Main Camera object.
6. Run the project to start generating images and coordinate labels.
7. Run `python collect_training_labels_to_csv.py <absolute_path_to_training_dir>` to aggregate all the generated labels into a single CSV file that RetinaNet can use
8. Run the [RetinaNet implementation](https://github.com/gittyeric/keras-retinanet) modified to support quadrilaterals instead of just bounding boxes.  The command I used for training was:

`python keras_retinanet/bin/train.py --points=4 --image-min-side=571 csv "path/to/training/annotations.csv" "path/to/training/classes.csv"`

This will start training and save each epoch to a snapshots folder.  Happy hacking!
