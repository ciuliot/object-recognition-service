#include <darknet.h>

// Adapted from https://github.com/pjreddie/darknet/blob/61c9d02ec461e30d55762ec7669d6a1d3c356fb2/examples/detector.c#L562

network *initialize(char *cfgfile, char *weightfile)
{
  image **alphabet = load_alphabet();
  network *net = load_network(cfgfile, weightfile, 0);
  set_batch_network(net, 1);

  return net;
}

void detect(network *net,
            char *datacfg,
            char *filename,
            float thresh,
            float hier_thresh,
            char *outfile)
{
  list *options = read_data_cfg(datacfg);
  char *name_list = option_find_str(options, "names", "data/names.list");
  char **names = get_labels(name_list);

  srand(2222222);
  char buff[256];
  char *input = buff;
  float nms = .45;
  strncpy(input, filename, 256);

  image im = load_image_color(input, 0, 0);
  image sized = letterbox_image(im, net->w, net->h);

  layer l = net->layers[net->n - 1];

  float *X = sized.data;
  network_predict(net, X);
  int nboxes = 0;
  detection *dets = get_network_boxes(net, im.w, im.h, thresh, hier_thresh, 0, 1, &nboxes);

  if (nms)
    do_nms_sort(dets, nboxes, l.classes, nms);

  FILE *output = fopen(outfile, "w");
  fprintf(output, "[\r\n");

  for (int i = 0; i < nboxes; ++i)
  {
    int class = -1;
    for (int j = 0; j < l.classes; ++j)
    {
      if (dets[i].prob[j] > thresh)
      {
        fprintf(output, "\t{\r\n");

        fprintf(output, "\t\t\"class\": \"%s\",\r\n", names[j]);
        fprintf(output, "\t\t\"probability\": %.2f,\r\n", dets[i].prob[j]);
        fprintf(output, "\t\t\"boundingBox\": { \r\n");

        box bbox = dets[i].bbox;
        fprintf(output, "\t\t\t\"top\": %.4f, \r\n", bbox.y);
        fprintf(output, "\t\t\t\"left\": %.4f, \r\n", bbox.x);
        fprintf(output, "\t\t\t\"width\": %.4f, \r\n", bbox.w);
        fprintf(output, "\t\t\t\"height\": %.4f \r\n", bbox.h);
        fprintf(output, "\t\t} \r\n");
        fprintf(output, "\t}%c\r\n", (i + 1) == nboxes ? ' ' : ',');
      }
    }
  }

  fprintf(output, "]\r\n");
  fclose(output);

  free_detections(dets, nboxes);

  free_image(im);
  free_image(sized);
}