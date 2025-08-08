using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Rest;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Threading.Tasks;
using System.Net;
using System.Linq;
using System.IO;
using SixLabors.ImageSharp.Formats;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;
using Discord.Interactions;
using System.Net.Http;
using SixLabors.ImageSharp.Formats.Png;
using Sketch_Bot.Models;

namespace Sketch_Bot.Modules
{
    public class ImageModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly HttpClient client = new HttpClient();
        private readonly IImageEncoder encoder = new PngEncoder();
        [SlashCommand("invert", "Inverts an image")]
        public async Task Invert(IAttachment inputImage)
        {
            await DeferAsync();
            try
            {
                var photoBytes = await client.GetByteArrayAsync(inputImage.Url);
                using (var stream = new MemoryStream())
                {
                    using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(photoBytes))
                    {
                        image.Mutate(x => x.Invert());
                        image.Save(stream, encoder);
                        image.Dispose();
                    }
                    stream.Position = 0;
                    string fileSize = HelperFunctions.FormatFileSize(stream.Length);
                    await Context.Interaction.FollowupWithFileAsync(stream, $"Invert.png", $"Filesize: `{fileSize}`");
                    await stream.FlushAsync();
                    stream.Dispose();
                }
            }
            catch (Exception ex)
            {
                await FollowupAsync($"Unable to download the image or verify the url\n{ex.GetType()}: {ex.Message}\n\n{ex.StackTrace}");
            }
        }
        [SlashCommand("grayscale", "Grayscales an image")]
        [Alias("greyscale", "gray", "grey")]
        public async Task Grayscale(IAttachment inputImage)
        {
            await DeferAsync();
            try
            {
                var photoBytes = await client.GetByteArrayAsync(inputImage.Url);
                using (var stream = new MemoryStream())
                {
                    using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(photoBytes))
                    {
                        image.Mutate(x => x.Grayscale());
                        image.Save(stream, encoder);
                    }
                    stream.Position = 0;
                    string fileSize = HelperFunctions.FormatFileSize(stream.Length);
                    await Context.Interaction.FollowupWithFileAsync(stream, $"Grayscale.png", $"Filesize: `{fileSize}`");
                    await stream.FlushAsync();
                    stream.Dispose();
                }
            }
            catch (Exception ex)
            {
                await FollowupAsync($"Unable to download the image or verify the url" +
                    $"\n{ex.GetType()}: {ex.Message}" +
                    $"\n" +
                    $"\n{ex.StackTrace}");
            }
        }
        [SlashCommand("flip", "Flips an image upside down")]
        public async Task Flip(IAttachment inputImage)
        {
            await DeferAsync();
            try
            {
                var photoBytes = await client.GetByteArrayAsync(inputImage.Url);
                using (var stream = new MemoryStream())
                {
                    using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(photoBytes))
                    {
                        image.Mutate(x => x.RotateFlip(RotateMode.Rotate180, FlipMode.Horizontal));
                        image.Save(stream, encoder);
                    }
                    stream.Position = 0;
                    string fileSize = HelperFunctions.FormatFileSize(stream.Length);
                    await Context.Interaction.FollowupWithFileAsync(stream, $"Flip.png", $"Filesize: `{fileSize}`");
                    await stream.FlushAsync();
                    stream.Dispose();
                }
            }
            catch (Exception ex)
            {
                await FollowupAsync($"Unable to download the image or verify the url\n{ex.GetType()}: {ex.Message}\n\n{ex.StackTrace}");
            }
        }
        [SlashCommand("sepia", "Sepia color filter")]
        public async Task Sepia(double amount, IAttachment inputImage)
        {
            await DeferAsync();
            if (amount > 1 && Context.Interaction.User.Id != 135446225565515776)
            {
                amount = 1;
            }
            try
            {
                var photoBytes = await client.GetByteArrayAsync(inputImage.Url);
                using (var stream = new MemoryStream())
                {
                    using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(photoBytes))
                    {
                        image.Mutate(x => x.Sepia((float)amount));
                        image.Save(stream, encoder);
                        image.Dispose();
                    }
                    stream.Position = 0;
                    string fileSize = HelperFunctions.FormatFileSize(stream.Length);
                    await Context.Interaction.FollowupWithFileAsync(stream, $"Sepia.png", $"Filesize: `{fileSize}`");
                    await stream.FlushAsync();
                    stream.Dispose();
                }
            }
            catch (Exception ex)
            {
                await FollowupAsync($"Unable to download the image or verify the url" +
                    $"\n{ex.GetType()}: {ex.Message}" +
                    $"\n{ex.StackTrace}");
            }
        }
        [SlashCommand("pixelate", "Pixelates an image")]
        public async Task Pixelate(int factor, IAttachment inputImage)
        {
            await DeferAsync();
            if (factor > 50 && Context.Interaction.User.Id != 135446225565515776)
            {
                factor = 50;
            }
            try
            {
                var photoBytes = await client.GetByteArrayAsync(inputImage.Url);
                using (var stream = new MemoryStream())
                {
                    using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(photoBytes))
                    {
                        image.Mutate(x => x.Pixelate(factor));
                        image.Save(stream, encoder);

                    }
                    stream.Position = 0;
                    string fileSize = HelperFunctions.FormatFileSize(stream.Length);
                    await Context.Interaction.FollowupWithFileAsync(stream, $"Pixelate.png", $"Factor: `{factor}`\nFilesize: `{fileSize}`");
                    await stream.FlushAsync();
                    stream.Dispose();
                }
            }
            catch (Exception ex)
            {
                await FollowupAsync($"Unable to download the image or verify the url\n{ex.GetType()}: {ex.Message}\n\n{ex.StackTrace}");
            }
        }
        [SlashCommand("contrast", "Adjusts the contrast of an image")]
        public async Task Contrast(float factor, IAttachment inputImage)
        {
            await DeferAsync();
            if (factor > 100 && Context.Interaction.User.Id != 135446225565515776)
            {
                factor = 100;
            }
            try
            {
                var photoBytes = await client.GetByteArrayAsync(inputImage.Url);
                using (var stream = new MemoryStream())
                {
                    using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(photoBytes))
                    {
                        image.Mutate(x => x.Contrast(factor));
                        image.Save(stream, encoder);

                    }
                    stream.Position = 0;
                    string fileSize = HelperFunctions.FormatFileSize(stream.Length);
                    await Context.Interaction.FollowupWithFileAsync(stream, $"Contrast.png", $"Factor: `{factor}`\n Filesize: `{fileSize}`");
                    await stream.FlushAsync();
                    stream.Dispose();
                }
            }
            catch (Exception ex)
            {
                await FollowupAsync($"Unable to download the image or verify the url\n{ex.GetType()}: {ex.Message}\n\n{ex.StackTrace}");
            }
        }
        [SlashCommand("glow", "Glows the image")]
        public async Task Glow(int size, IAttachment inputImage)
        {
            await DeferAsync();
            if (size > 2500 && Context.Interaction.User.Id != 135446225565515776)
            {
                size = 2500;
            }
            try
            {
                var photoBytes = await client.GetByteArrayAsync(inputImage.Url);
                using (var stream = new MemoryStream())
                {
                    using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(photoBytes))
                    {
                        image.Mutate(x => x.Glow(size));
                        image.Save(stream, encoder);

                    }
                    stream.Position = 0;
                    string fileSize = HelperFunctions.FormatFileSize(stream.Length);
                    await Context.Interaction.FollowupWithFileAsync(stream, $"Glow.png", $"Factor: `{size}`\nFilesize: `{fileSize}`");
                    await stream.FlushAsync();
                    stream.Dispose();
                }
            }
            catch (Exception ex)
            {
                await FollowupAsync($"Unable to download the image or verify the url\n{ex.GetType()}: {ex.Message}\n\n{ex.StackTrace}");
            }
        }
        [SlashCommand("skew", "Skews the image")]
        public async Task Skew(float x, float y, IAttachment inputImage)
        {
            await DeferAsync();
            try
            {
                var photoBytes = await client.GetByteArrayAsync(inputImage.Url);
                using (var stream = new MemoryStream())
                {
                    using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(photoBytes))
                    {
                        image.Mutate(m => m.Skew(x, y));
                        image.Save(stream, encoder);

                    }
                    stream.Position = 0;
                    string fileSize = HelperFunctions.FormatFileSize(stream.Length);
                    await Context.Interaction.FollowupWithFileAsync(stream, $"Skew.png", $"X: `{x}` Y: `{y}`\nFilesize: `{fileSize}`");
                    await stream.FlushAsync();
                    stream.Dispose();
                }
            }
            catch (Exception ex)
            {
                await FollowupAsync($"Unable to download the image or verify the url\n{ex.GetType()}: {ex.Message}\n\n{ex.StackTrace}");
            }
        }
        [SlashCommand("oil", "Oil painting filter")]
        public async Task Oil(IAttachment inputImage)
        {
            await DeferAsync();
            try
            {
                var photoBytes = await client.GetByteArrayAsync(inputImage.Url);
                using (var stream = new MemoryStream())
                {
                    using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(photoBytes))
                    {
                        image.Mutate(x => x.OilPaint());
                        image.Save(stream, encoder);

                    }
                    stream.Position = 0;
                    string fileSize = HelperFunctions.FormatFileSize(stream.Length);
                    await Context.Interaction.FollowupWithFileAsync(stream, $"Oil.png", $"Filesize: `{fileSize}`");
                    await stream.FlushAsync();
                    stream.Dispose();
                }
            }
            catch (Exception ex)
            {
                await FollowupAsync($"Unable to download the image or verify the url\n{ex.GetType()}: {ex.Message}\n\n{ex.StackTrace}");
            }
        }
        [SlashCommand("polaroid", "Polaroid photo filter")]
        public async Task Polaroid(IAttachment inputImage)
        {
            await DeferAsync();
            try
            {
                var photoBytes = await client.GetByteArrayAsync(inputImage.Url);
                using (var stream = new MemoryStream())
                {
                    using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(photoBytes))
                    {
                        image.Mutate(x => x.Polaroid());
                        image.Save(stream, encoder);

                    }
                    stream.Position = 0;
                    string fileSize = HelperFunctions.FormatFileSize(stream.Length);
                    await Context.Interaction.FollowupWithFileAsync(stream, $"Polaroid.png", $"Filesize: `{fileSize}`");
                    await stream.FlushAsync();
                    stream.Dispose();
                }
            }
            catch (Exception ex)
            {
                await FollowupAsync($"Unable to download the image or verify the url\n{ex.GetType()}: {ex.Message}\n\n{ex.StackTrace}");
            }
        }
        [SlashCommand("hue", "Alters the Hue component of a image")]
        public async Task Hue(float degrees, IAttachment inputImage)
        {
            await DeferAsync();
            try
            {
                var photoBytes = await client.GetByteArrayAsync(inputImage.Url);
                using (var stream = new MemoryStream())
                {
                    using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(photoBytes))
                    {
                        image.Mutate(x => x.Hue(degrees));
                        image.Save(stream, encoder);

                    }
                    stream.Position = 0;
                    string fileSize = HelperFunctions.FormatFileSize(stream.Length);
                    await Context.Interaction.FollowupWithFileAsync(stream, $"Hue.png", $"Degrees: `{degrees}`\nFilesize: `{fileSize}`");
                    await stream.FlushAsync();
                    stream.Dispose();
                }
            }
            catch (Exception ex)
            {
                await FollowupAsync($"Unable to download the image or verify the url\n{ex.GetType()}: {ex.Message}\n\n{ex.StackTrace}");
            }
        }
        [SlashCommand("huewheel", "Creates a GIF cycling the hue of the image through 360 degrees over the given seconds")]
        public async Task Huewheel(int seconds, IAttachment inputImage)
        {
            await DeferAsync();
            if (seconds < 1 || seconds > 10)
            {
                await FollowupAsync("Seconds must be between `1` and `10`");
                return;
            }
            try
            {
                var photoBytes = await client.GetByteArrayAsync(inputImage.Url);

                int fps = 30;
                int frameCount = seconds * fps;
                int frameDelay = 100 / fps; // in 1/100s of a second

                using var original = SixLabors.ImageSharp.Image.Load<Rgba32>(photoBytes);

                var gifMetaData = original.Metadata.GetGifMetadata();
                gifMetaData.RepeatCount = 0; // infinite loop

                // Prepare the first frame (0 degrees)
                original.Mutate(x => x.Hue(0));
                var rootFrameMeta = original.Frames.RootFrame.Metadata.GetGifMetadata();
                rootFrameMeta.FrameDelay = frameDelay;

                // Generate and add hue-shifted frames
                for (int i = 1; i < frameCount; i++)
                {
                    float degrees = i * (360f / frameCount);
                    using var frame = original.Clone(ctx => ctx.Hue(degrees));
                    var meta = frame.Frames.RootFrame.Metadata.GetGifMetadata();
                    meta.FrameDelay = frameDelay;
                    original.Frames.AddFrame(frame.Frames.RootFrame);
                }

                using var gifStream = new MemoryStream();
                original.SaveAsGif(gifStream);
                gifStream.Position = 0;
                string fileSize = HelperFunctions.FormatFileSize(gifStream.Length);
                await Context.Interaction.FollowupWithFileAsync(gifStream, "Huewheel.gif", $"Hue wheel\n`{seconds}` second(s)\nFrame count: `{frameCount}`\nFilesize: `{fileSize}`");
                await gifStream.FlushAsync();
            }
            catch (Exception ex)
            {
                await FollowupAsync($"Unable to process the image\n{ex.GetType()}: {ex.Message}\n\n{ex.StackTrace}");
            }
        }
        [SlashCommand("opacity", "Multiplies the opacity of the input image with a given factor between 0 and 1")]
        public async Task Opacity(float factor, IAttachment inputImage)
        {
            await DeferAsync();
            if (factor < 0 || factor > 1)
            {
                await FollowupAsync("Factor must be between `0` and `1`");
                return;
            }
            try
            {
                var photoBytes = await client.GetByteArrayAsync(inputImage.Url);
                using (var stream = new MemoryStream())
                {
                    using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(photoBytes))
                    {
                        image.Mutate(x => x.Opacity(factor));
                        image.Save(stream, encoder);

                    }
                    stream.Position = 0;
                    string fileSize = HelperFunctions.FormatFileSize(stream.Length);
                    await Context.Interaction.FollowupWithFileAsync(stream, $"Opacity.png", $"Filesize: `{fileSize}`");
                    await stream.FlushAsync();
                    stream.Dispose();
                }
            }
            catch (Exception ex)
            {
                await FollowupAsync($"Unable to download the image or verify the url\n{ex.GetType()}: {ex.Message}\n\n{ex.StackTrace}");
            }
        }
        [SlashCommand("fadeout", "Creates a GIF fading out the image to transparency")]
        public async Task Fadeout(int seconds, IAttachment inputImage)
        {
            await DeferAsync();
            if (seconds < 1 || seconds > 100)
            {
                await FollowupAsync("Seconds must be between `1` and `100`");
                return;
            }
            try
            {
                var photoBytes = await client.GetByteArrayAsync(inputImage.Url);

                using var original = SixLabors.ImageSharp.Image.Load<Rgba32>(photoBytes);

                int frameCount = 256;
                int frameDelay = (int)Math.Ceiling(seconds * 100.0 / frameCount); // in 1/100s

                var gifMetaData = original.Metadata.GetGifMetadata();
                gifMetaData.RepeatCount = 0; // infinite loop

                var rootFrameMeta = original.Frames.RootFrame.Metadata.GetGifMetadata();
                rootFrameMeta.FrameDelay = frameDelay;

                // Generate and add fadeout frames
                for (int i = 0; i < frameCount; i++)
                {
                    float t = i / (float)(frameCount - 1);
                    using var frame = original.Clone();
                    frame.ProcessPixelRows(accessor =>
                    {
                        for (int y = 0; y < accessor.Height; y++)
                        {
                            Span<Rgba32> row = accessor.GetRowSpan(y);
                            for (int x = 0; x < row.Length; x++)
                            {
                                var orig = row[x];
                                var bg = new Rgba32(255, 255, 255, 255); // White background
                                row[x] = new Rgba32(
                                    (byte)(orig.R * (1 - t) + bg.R * t),
                                    (byte)(orig.G * (1 - t) + bg.G * t),
                                    (byte)(orig.B * (1 - t) + bg.B * t),
                                    255 // GIFs are opaque
                                );
                            }
                        }
                    });
                    // Add frame to GIF as before
                }

                using var gifStream = new MemoryStream();
                original.SaveAsGif(gifStream);
                gifStream.Position = 0;
                string fileSize = HelperFunctions.FormatFileSize(gifStream.Length);
                await Context.Interaction.FollowupWithFileAsync(gifStream, "Fadeout.gif", $"Fadeout duration: `{seconds}` second(s)\nFilesize: `{fileSize}`");
                await gifStream.FlushAsync();
            }
            catch (Exception ex)
            {
                await FollowupAsync($"Unable to process the image\n{ex.GetType()}: {ex.Message}\n\n{ex.StackTrace}");
            }
        }
        [SlashCommand("spin", "Creates a GIF of the image spinning 360 degrees over the given seconds")]
        public async Task Spin(int seconds, IAttachment inputImage)
        {
            await DeferAsync();
            if (seconds < 1 || seconds > 20)
            {
                await FollowupAsync("Seconds must be between `1` and `20`");
                return;
            }
            try
            {
                var photoBytes = await client.GetByteArrayAsync(inputImage.Url);

                int fps = 30;
                int frameCount = seconds * fps;
                int frameDelay = 100 / fps; // in 1/100s of a second

                using var original = SixLabors.ImageSharp.Image.Load<Rgba32>(photoBytes);
                int ow = original.Width;
                int oh = original.Height;
                int canvasSize = Math.Max(ow, oh);
                int shortest = Math.Min(ow, oh);

                using var gif = new Image<Rgba32>(canvasSize, canvasSize, new Rgba32(0, 0, 0, 0));
                var gifMetaData = gif.Metadata.GetGifMetadata();
                gifMetaData.RepeatCount = 0; // infinite loop

                for (int i = 0; i < frameCount; i++)
                {
                    float degrees = i * (360f / frameCount);

                    using var canvas = new Image<Rgba32>(canvasSize, canvasSize, new Rgba32(0, 0, 0, 0));
                    var clone = original.Clone(ctx => ctx.Rotate(degrees));
                    canvas.Mutate(ctx => ctx.DrawImage(clone, new Point(0, 0), 1f));

                    if (i == 0)
                    {
                        gif.Mutate(ctx => ctx.DrawImage(canvas, new Point(0, 0), 1f));
                        gif.Frames.RootFrame.Metadata.GetGifMetadata().FrameDelay = frameDelay;
                    }
                    else
                    {
                        var meta = canvas.Frames.RootFrame.Metadata.GetGifMetadata();
                        meta.FrameDelay = frameDelay;
                        gif.Frames.AddFrame(canvas.Frames.RootFrame);
                    }
                }

                using var gifStream = new MemoryStream();
                gif.SaveAsGif(gifStream);
                gifStream.Position = 0;
                string fileSize = HelperFunctions.FormatFileSize(gifStream.Length);
                await Context.Interaction.FollowupWithFileAsync(gifStream, "Spin.gif", $"Spin\n`{seconds}` second(s)\nFrame count: `{frameCount}`\nFilesize: `{fileSize}`");
                await gifStream.FlushAsync();
            }
            catch (Exception ex)
            {
                await FollowupAsync($"Unable to process the image\n{ex.GetType()}: {ex.Message}\n\n{ex.StackTrace}");
            }
        }
        [SlashCommand("detectedges", "Detect edges on an image")]
        public async Task DetectEdges(IAttachment inputImage)
        {
            await DeferAsync();
            try
            {
                var photoBytes = await client.GetByteArrayAsync(inputImage.Url);
                using (var stream = new MemoryStream())
                {
                    using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(photoBytes))
                    {
                        image.Mutate(x => x.DetectEdges());
                        image.Save(stream, encoder);

                    }
                    stream.Position = 0;
                    string fileSize = HelperFunctions.FormatFileSize(stream.Length);
                    await Context.Interaction.FollowupWithFileAsync(stream, $"DetectEdges.png", $"Filesize: `{fileSize}`");
                    await stream.FlushAsync();
                    stream.Dispose();
                }
            }
            catch (Exception ex)
            {
                await FollowupAsync($"Unable to download the image or verify the url\n{ex.GetType()}: {ex.Message}\n\n{ex.StackTrace}");
            }
        }
        [SlashCommand("crop", "Crops the image")]
        public async Task Crop(int width, int height, IAttachment inputImage)
        {
            await DeferAsync();
            try
            {
                var photoBytes = await client.GetByteArrayAsync(inputImage.Url);
                using (var stream = new MemoryStream())
                {
                    using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(photoBytes))
                    {
                        image.Mutate(x => x.Crop(width, height));
                        image.Save(stream, encoder);

                    }
                    stream.Position = 0;
                    if (width > 3840 || height > 2160)
                    {
                        await FollowupAsync("Image is larger than `3840x2160`");
                        stream.Dispose();
                        return;
                    }
                    string fileSize = HelperFunctions.FormatFileSize(stream.Length);
                    await Context.Interaction.FollowupWithFileAsync(stream, $"Crop.png", $"`{width}`x`{height}`\nFilesize: `{fileSize}`");
                    await stream.FlushAsync();
                    stream.Dispose();
                }
            }
            catch (Exception ex)
            {
                await FollowupAsync($"Unable to download the image or verify the url\n{ex.GetType()}: {ex.Message}\n\n{ex.StackTrace}");
            }
        }
        [SlashCommand("brightness", "Adjusts brightness of an image")]
        public async Task Brightness(float factor, IAttachment inputImage)
        {
            await DeferAsync();
            if (factor > 100 && Context.Interaction.User.Id != 135446225565515776)
            {
                factor = 100;
            }
            try
            {
                var photoBytes = await client.GetByteArrayAsync(inputImage.Url);
                using (var stream = new MemoryStream())
                {
                    using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(photoBytes))
                    {
                        image.Mutate(x => x.Brightness(factor));
                        image.Save(stream, encoder);

                    }
                    stream.Position = 0;
                    string fileSize = HelperFunctions.FormatFileSize(stream.Length);
                    await Context.Interaction.FollowupWithFileAsync(stream, $"Brightness.png", $"Factor: `{factor}`\nFilesize: `{fileSize}`");
                    await stream.FlushAsync();
                    stream.Dispose();
                }
            }
            catch (Exception ex)
            {
                await FollowupAsync($"Unable to download the image or verify the url\n{ex.GetType()}: {ex.Message}\n\n{ex.StackTrace}");
            }
        }
        [SlashCommand("rotate", "Rotates an image")]
        public async Task Rotate(float angle, IAttachment inputImage)
        {
            await DeferAsync();
            try
            {
                var photoBytes = await client.GetByteArrayAsync(inputImage.Url);
                using (var stream = new MemoryStream())
                {
                    using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(photoBytes))
                    {
                        image.Mutate(x => x.Rotate(angle));
                        image.Save(stream, encoder);

                    }
                    stream.Position = 0;
                    string fileSize = HelperFunctions.FormatFileSize(stream.Length);
                    await Context.Interaction.FollowupWithFileAsync(stream, $"Rotate.png", $"Degrees `{angle}`\nFilesize: `{fileSize}`");
                    await stream.FlushAsync();
                    stream.Dispose();
                }
            }
            catch (Exception ex)
            {
                await FollowupAsync($"Unable to download the image or verify the url\n{ex.GetType()}: {ex.Message}\n\n{ex.StackTrace}");
            }
        }
        [SlashCommand("saturate", "Saturates an image")]
        public async Task Saturate(float size, IAttachment inputImage)
        {
            await DeferAsync();
            if (size > 100 && Context.Interaction.User.Id != 135446225565515776)
            {
                size = 100;
            }
            try
            {
                var photoBytes = await client.GetByteArrayAsync(inputImage.Url);
                using (var stream = new MemoryStream())
                {
                    using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(photoBytes))
                    {
                        image.Mutate(x => x.Saturate(size));
                        image.Save(stream, encoder);
                    }
                    stream.Position = 0;
                    string fileSize = HelperFunctions.FormatFileSize(stream.Length);
                    await Context.Interaction.FollowupWithFileAsync(stream, $"Saturate.png", $"Factor: `{size}`\nFilesize: `{fileSize}`");
                    await stream.FlushAsync();
                    stream.Dispose();
                }
            }
            catch (Exception ex)
            {
                await FollowupAsync($"Unable to download the image or verify the url\n{ex.GetType()}: {ex.Message}\n\n{ex.StackTrace}");
            }
        }
        [SlashCommand("blur", "Blurs an image")]
        public async Task Blur(int factor, IAttachment inputImage)
        {
            await DeferAsync();
            if (factor > 100 && Context.Interaction.User.Id != 135446225565515776)
            {
                factor = 100;
            }
            try
            {
                var photoBytes = await client.GetByteArrayAsync(inputImage.Url);
                using (var stream = new MemoryStream())
                {
                    using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(photoBytes))
                    {
                        image.Mutate(x => x.BoxBlur(factor));
                        image.Save(stream, encoder);
                    }
                    stream.Position = 0;
                    string fileSize = HelperFunctions.FormatFileSize(stream.Length);
                    await Context.Interaction.FollowupWithFileAsync(stream, $"Blur.png", $"Factor: `{factor}`\nFilesize: `{fileSize}`");
                    await stream.FlushAsync();
                    stream.Dispose();
                }
            }
            catch (Exception ex)
            {
                await FollowupAsync($"Unable to download the image or verify the url\n{ex.GetType()}: {ex.Message}\n\n{ex.StackTrace}");
            }
        }
        [SlashCommand("sharpen", "Sharpens an image")]
        public async Task Sharpen(int size, IAttachment inputImage)
        {
            await DeferAsync();
            if (size > 100 && Context.Interaction.User.Id != 135446225565515776)
            {
                size = 100;
            }
            try
            {
                var photoBytes = await client.GetByteArrayAsync(inputImage.Url);
                using (var stream = new MemoryStream())
                {
                    using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(photoBytes))
                    {
                        image.Mutate(x => x.GaussianSharpen(size));
                        image.Save(stream, encoder);

                        image.Dispose();
                    }
                    stream.Position = 0;
                    string fileSize = HelperFunctions.FormatFileSize(stream.Length);
                    await Context.Interaction.FollowupWithFileAsync(stream, $"Sharpen.png", $"Factor: `{size}`\nFilesize: `{fileSize}`");
                    await stream.FlushAsync();
                    stream.Dispose();
                }
            }
            catch (Exception ex)
            {
                await FollowupAsync($"Unable to download the image or verify the url\n{ex.GetType()}: {ex.Message}\n\n{ex.StackTrace}");
            }
        }
        [SlashCommand("upscale", "Upscales an image")]
        public async Task Upscale(IAttachment inputImage)
        {
            await DeferAsync();
            try
            {
                int width;
                int height;

                var photoBytes = await client.GetByteArrayAsync(inputImage.Url);
                using (var stream = new MemoryStream())
                {
                    using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(photoBytes))
                    {
                        if (image.Width > 3840 || image.Height > 2160)
                        {
                            await FollowupAsync("Image is larger than `3840x2160`");
                            image.Dispose();
                        }
                        else
                        {
                            image.Mutate(x => x.Resize(image.Width * 2, image.Height * 2));
                            image.Save(stream, encoder);

                            width = Math.Min(image.Width, 3840);
                            height = Math.Min(image.Height, 2160);
                            image.Dispose();
                            stream.Position = 0;
                            string fileSize = HelperFunctions.FormatFileSize(stream.Length);
                            await Context.Interaction.FollowupWithFileAsync(stream, $"Upscale.png", $"Image upscaled by `2` (`{width}x{height}`)\nFilesize: `{fileSize}`");
                            await stream.FlushAsync();
                            stream.Dispose();
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                await FollowupAsync($"Unable to download the image or verify the url\n{ex.GetType()}: {ex.Message}\n\n{ex.StackTrace}");
            }
        }
        [SlashCommand("resize", "Resizes the image")]
        public async Task Resize(int width, int height, IAttachment inputImage)
        {
            await DeferAsync();
            if (width > 3840 && Context.Interaction.User.Id != 135446225565515776)
            {
                width = 3840;
            }
            if (height > 2160 && Context.Interaction.User.Id != 135446225565515776)
            {
                height = 2160;
            }
            try
            {
                var photoBytes = await client.GetByteArrayAsync(inputImage.Url);
                using (var stream = new MemoryStream())
                {
                    using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(photoBytes))
                    {
                        image.Mutate(x => x.Resize(width, height));
                        image.Save(stream, encoder);

                        image.Dispose();
                    }
                    stream.Position = 0;
                    string fileSize = HelperFunctions.FormatFileSize(stream.Length);
                    await Context.Interaction.FollowupWithFileAsync(stream, $"Resize.png", $"Image resized to `{width}x{height}`\nFilesize: `{fileSize}`");
                    await stream.FlushAsync();
                    stream.Dispose();
                }
            }
            catch (Exception ex)
            {
                await FollowupAsync($"Unable to download the image or verify the url\n{ex.GetType()}: {ex.Message}\n\n{ex.StackTrace}");
            }
        }
    }
}
