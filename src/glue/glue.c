#include <darknet.h>

// Adapted from https://github.com/pjreddie/darknet/blob/61c9d02ec461e30d55762ec7669d6a1d3c356fb2/examples/detector.c#L562

network *initialize(char *cfgfile, char *weightfile, int cuda_device)
{
#ifdef GPU
  cuda_set_device(cuda_device);
#endif
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

void train_detector(char *train_images, 
  char *cfgfile, 
  char *weightfile, 
  char *outputdir, 
  int *gpus, 
  int ngpus, 
  int clear,
  void(* batch_finished_callback)(size_t batch_number, float loss, float avg_loss, float learning_rate, int images))
{
    srand(time(0));
    char *base = basecfg(cfgfile);
    printf("%s\n", base);
    float avg_loss = -1;
    network **nets = calloc(ngpus, sizeof(network));

    srand(time(0));
    int seed = rand();
    int i;
    for(i = 0; i < ngpus; ++i){
        srand(seed);
#ifdef GPU
        cuda_set_device(gpus[i]);
#endif
        nets[i] = load_network(cfgfile, weightfile, clear);
        nets[i]->learning_rate *= ngpus;
    }
    srand(time(0));
    network *net = nets[0];

    int imgs = net->batch * net->subdivisions * ngpus;
    fprintf(stderr, "Learning Rate: %g, Momentum: %g, Decay: %g\n", net->learning_rate, net->momentum, net->decay);
    data train, buffer;

    layer l = net->layers[net->n - 1];

    int classes = l.classes;
    float jitter = l.jitter;

    list *plist = get_paths(train_images);
    //int N = plist->size;
    char **paths = (char **)list_to_array(plist);

    load_args args = get_base_args(net);
    args.coords = l.coords;
    args.paths = paths;
    args.n = imgs;
    args.m = plist->size;
    args.classes = classes;
    args.jitter = jitter;
    args.num_boxes = l.max_boxes;
    args.d = &buffer;
    args.type = DETECTION_DATA;
    //args.type = INSTANCE_DATA;
    args.threads = 64;

    pthread_t load_thread = load_data(args);
    double time;
    int count = 0;
    //while(i*imgs < N*120){
    while(get_current_batch(net) < net->max_batches){
        if(l.random && count++%10 == 0){
            fprintf(stderr, "Resizing\n");
            int dim = (rand() % 10 + 10) * 32;
            if (get_current_batch(net)+200 > net->max_batches) dim = 608;
            //int dim = (rand() % 4 + 16) * 32;
            fprintf(stderr, "%d\n", dim);
            args.w = dim;
            args.h = dim;

            pthread_join(load_thread, 0);
            train = buffer;
            free_data(train);
            load_thread = load_data(args);

            #pragma omp parallel for
            for(i = 0; i < ngpus; ++i){
                resize_network(nets[i], dim, dim);
            }
            net = nets[0];
        }
        time=what_time_is_it_now();
        pthread_join(load_thread, 0);
        train = buffer;
        load_thread = load_data(args);

        fprintf(stderr, "Loaded: %lf seconds\n", what_time_is_it_now()-time);

        time=what_time_is_it_now();
        float loss = 0;
#ifdef GPU
        if(ngpus == 1){
            loss = train_network(net, train);
        } else {
            loss = train_networks(nets, ngpus, train, 4);
        }
#else
        loss = train_network(net, train);
#endif
        if (avg_loss < 0) avg_loss = loss;
        avg_loss = avg_loss*.9 + loss*.1;

        i = get_current_batch(net);
        //fprintf(stderr, "%ld: %f, %f avg, %f rate, %lf seconds, %d images\n", get_current_batch(net), loss, avg_loss, get_current_rate(net), what_time_is_it_now()-time, i*imgs);

        (*batch_finished_callback)(get_current_batch(net), loss, avg_loss, get_current_rate(net), i*imgs);
        if(i%100==0){
#ifdef GPU
            if(ngpus != 1) sync_nets(nets, ngpus, 0);
#endif
            char buff[256];
            sprintf(buff, "%s/%s.backup", outputdir, base);
            save_weights(net, buff);
        }
        if(i%10000==0 || (i < 1000 && i%100 == 0)){
#ifdef GPU
            if(ngpus != 1) sync_nets(nets, ngpus, 0);
#endif
            char buff[256];
            sprintf(buff, "%s/%s_%d.weights", outputdir, base, i);
            save_weights(net, buff);
        }
        free_data(train);
    }
#ifdef GPU
    if(ngpus != 1) sync_nets(nets, ngpus, 0);
#endif
    char buff[256];
    sprintf(buff, "%s/%s_final.weights", outputdir, base);
    save_weights(net, buff);
}