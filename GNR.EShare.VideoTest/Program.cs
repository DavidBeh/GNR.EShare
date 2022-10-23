// See https://aka.ms/new-console-template for more information


using System.Diagnostics;
using Windows.Graphics.Imaging;
using Windows.Media.Capture.Frames;
using Windows.Media.Core;
using Windows.Media.Editing;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Streams;
using ABI.Windows.Foundation;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Buffer = Windows.Storage.Streams.Buffer;
Stopwatch stp = new Stopwatch();

void StpLog(string text) => Console.WriteLine($"{text}: {stp.ElapsedMilliseconds}ms");
stp.Start();
var fi = new FileInfo("hat.mp4");
Console.WriteLine($"Exists: {fi.Exists}");
StorageFile inputFile = await StorageFile.GetFileFromPathAsync(fi.FullName);

var outputDir = Directory.CreateDirectory("frames");
outputDir.Delete(true);
outputDir.Create();
var outputFolder = await StorageFolder.GetFolderFromPathAsync(outputDir.FullName);

MediaPlayer player = new MediaPlayer();
var m = MediaSource.CreateFromStorageFile(inputFile);

var fileMeta = await inputFile.Properties.GetVideoPropertiesAsync();

player.Source = MediaSource.CreateFromStorageFile(inputFile);

var softwareBitmap = new SoftwareBitmap(BitmapPixelFormat.Bgra8, (int)fileMeta.Width, (int)fileMeta.Height, BitmapAlphaMode.Premultiplied);

//var sof = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), (int)fileMeta.Width, (int)fileMeta.Height, 96);
//var c = CanvasBitmap.CreateFromDirect3D11Surface(CanvasDevice.GetSharedDevice(), canvasRenderTarget);

player.Play();
await Task.Delay(2000);
player.IsVideoFrameServerEnabled = true;
player.VideoFrameAvailable += (sender, o) =>
{
    var canvasBitmap = CanvasBitmap.CreateFromSoftwareBitmap(CanvasDevice.GetSharedDevice(), softwareBitmap);
    Console.WriteLine("frame created");
    player.CopyFrameToVideoSurface(canvasBitmap);
    var frame = player.Position;
    Task.Run(async () =>
    {
        try
        {
            var file = await outputFolder.CreateFileAsync($"{frame.Minutes:00}_{frame.Seconds:00}_{frame.Milliseconds:0000}.jpeg", CreationCollisionOption.ReplaceExisting);
            var outputStream = await file.OpenAsync(FileAccessMode.ReadWrite);
            var bitmap = await SoftwareBitmap.CreateCopyFromSurfaceAsync(canvasBitmap);
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, outputStream);
        
            encoder.SetSoftwareBitmap(bitmap);
        
            await encoder.FlushAsync();
            await outputStream.FlushAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

    });
};

Console.ReadLine();

#region Composition
/*

var mediaClip = await MediaClip.CreateFromFileAsync(inputFile);
var mediaComposition = new MediaComposition();
// Output
mediaComposition.Clips.Add(mediaClip);
StpLog("composition created");
var clipEncoding = mediaComposition.Clips.First().GetVideoEncodingProperties();
var thumb = await mediaComposition.GetThumbnailAsync(TimeSpan.FromSeconds(70), (int)clipEncoding.Width,
    (int)clipEncoding.Height, VideoFramePrecision.NearestKeyFrame);
StpLog("thumbnail created");

// Decode Input
var decoder = await BitmapDecoder.CreateAsync(thumb);
var detached = (await decoder.GetPixelDataAsync()).DetachPixelData();
StpLog("decoded");

// Ouput
var outputFileInfo = new FileInfo("fr1.jpeg");
var outputFolder = await StorageFolder.GetFolderFromPathAsync(outputFileInfo.Directory!.FullName);
var outputFile = await outputFolder.CreateFileAsync("fr1.jpeg", CreationCollisionOption.ReplaceExisting);
using var outputStream = await outputFile.OpenAsync(FileAccessMode.ReadWrite);

var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, outputStream);
encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, decoder.OrientedPixelWidth,
    decoder.OrientedPixelHeight, decoder.DpiX, decoder.DpiY, detached);

await encoder.FlushAsync();
await outputStream.FlushAsync();
SoftwareBitmap b = null;
*/
#endregion

