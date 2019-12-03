using System;
using System.Runtime.InteropServices;

namespace Digitalist.ObjectRecognition
{
  internal static class NativeMethods
  {
    public static class Darknet
    {
      public delegate void batch_finished_callback(ulong batch_number, float loss, 
        float avg_loss, float learning_rate, int images);

      [DllImport("libdarknet_glue")]
      public static extern IntPtr detect(
        IntPtr network,
        [MarshalAs(UnmanagedType.LPStr)] string datacfg,
        [MarshalAs(UnmanagedType.LPStr)] string filename,
        float thresh,
        float hier_thresh,
        [MarshalAs(UnmanagedType.LPStr)] string outputFile);

      [DllImport("libdarknet_glue")]
      public static extern IntPtr initialize(
        [MarshalAs(UnmanagedType.LPStr)] string cfgfile,
        [MarshalAs(UnmanagedType.LPStr)] string weightfile,
        int cuda_device);

      [DllImport("libdarknet_glue")]
      public static extern void train_detector(
        [MarshalAs(UnmanagedType.LPStr)] string train_images, 
        [MarshalAs(UnmanagedType.LPStr)] string cfgfile, 
        [MarshalAs(UnmanagedType.LPStr)] string weightfile, 
        [MarshalAs(UnmanagedType.LPStr)] string outputdir, 
        [In, Out] int[] gpus, int ngpus, int clear,
        batch_finished_callback batch_finished_callback);
    }
  }
}
