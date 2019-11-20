using System;
using System.Runtime.InteropServices;

namespace Digitalist.ObjectRecognition
{
  internal static class NativeMethods
  {
    public static class Darknet
    {
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
        [MarshalAs(UnmanagedType.LPStr)] string weightfile);
    }
  }

}