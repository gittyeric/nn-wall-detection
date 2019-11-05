import json
import sys
import os

img_width = 800
img_height = 800


def jsonFileToCsvRow(dir, index, json_obj):
    row = [dir + 'imgs/' + index + '.jpg']
    for point in json_obj:
        row.append(str(int(point[0] * img_width)))
        row.append(str(int(point[1] * img_height)))
    row.append("wall")
    return ",".join(row)


def main():
    args = sys.argv[1:]
    dir = args[0]
    print ("Collecting labels from " + dir)
    label_files = os.listdir(dir + 'labels/')
    print ('Got ' + str(len(label_files)) + ' files')

    # Create CSV
    csvFile = open(dir + 'annotations.csv', "w+")

    for filename in label_files:
        file = open(dir + "labels/" + filename, "r")
        index = filename.split('/')[-1].split(".")[0]
        json_str = file.read()
        points = json.loads(json_str)
        csvFile.write(jsonFileToCsvRow(dir, index, points) + "\n")
        print ("Wrote " + index)
        file.close()

    csvFile.close()
    print ("Done")


if __name__ == '__main__':
    main()