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
using SixLabors.ImageSharp.Formats.Png;
using System.Net.Http;

namespace Sketch_Bot.Modules
{
    public class ImageModuleOld : ModuleBase<SocketCommandContext>
    {
        private readonly HttpClient client = new HttpClient();
        private readonly IImageEncoder encoder = new PngEncoder();

        [Command("invert", RunMode = RunMode.Async)]
        public async Task Invert([Remainder] string fileUrl = "")
        {
            if (Context.Message.Attachments.Count > 0)
            {
                fileUrl = Context.Message.Attachments.FirstOrDefault().Url;
            }
            if (fileUrl != "")
            {
                using (Context.Channel.EnterTypingState())
                {
                    try
                    {
                        
                        var photoBytes = await client.GetByteArrayAsync(fileUrl);
                        using (var stream = new MemoryStream())
                        {
                            using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(photoBytes))
                            {
                                image.Mutate(x => x.Invert());
                                image.Save(stream, encoder);
    
                                image.Dispose();
                            }
                            stream.Position = 0;
                            await Context.Channel.SendFileAsync(stream, $"Invert.png");
                            await stream.FlushAsync();
                            stream.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        await ReplyAsync($"Unable to download the image or verify the url" +
                            $"\n{ex.GetType().ToString()}: {ex.Message}" +
                            $"\n" +
                            $"\n{ex.StackTrace}");
                    }
                }
            }
            else
            {
                await ReplyAsync("No file or Url provided");
            }
        }
        [Command("grayscale", RunMode = RunMode.Async)]
        [Alias("greyscale", "gray", "grey")]
        public async Task Grayscale([Remainder] string fileUrl = "")
        {
            if (Context.Message.Attachments.Count > 0)
            {
                fileUrl = Context.Message.Attachments.FirstOrDefault().Url;
            }
            if (fileUrl != "")
            {
                using (Context.Channel.EnterTypingState())
                {
                    try
                    {
                        
                        var photoBytes = await client.GetByteArrayAsync(fileUrl);
                        using (var stream = new MemoryStream())
                        {
                            using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(photoBytes))
                            {
                                image.Mutate(x => x.Grayscale());
                                image.Save(stream, encoder);
    
                            }
                            stream.Position = 0;
                            await Context.Channel.SendFileAsync(stream, $"Grayscale.png");
                            await stream.FlushAsync();
                        }
                    }
                    catch (Exception)
                    {
                        await ReplyAsync($"Unable to download the image or verify the url");
                    }
                }
            }
            else
            {
                await ReplyAsync("No file or Url provided");
            }
        }
        [Command("flip", RunMode = RunMode.Async)]
        public async Task Flip([Remainder] string fileUrl = "")
        {
            if (Context.Message.Attachments.Count > 0)
            {
                fileUrl = Context.Message.Attachments.FirstOrDefault().Url;
            }
            if (fileUrl != "")
            {
                using (Context.Channel.EnterTypingState())
                {
                    try
                    {
                        
                        var photoBytes = await client.GetByteArrayAsync(fileUrl);
                        using (var stream = new MemoryStream())
                        {
                            using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(photoBytes))
                            {
                                image.Mutate(x => x.RotateFlip(RotateMode.Rotate180, FlipMode.None));
                                image.Save(stream, encoder);
    
                            }
                            stream.Position = 0;
                            await Context.Channel.SendFileAsync(stream, $"Flip.png");
                            await stream.FlushAsync();
                        }
                    }
                    catch (Exception)
                    {
                        await ReplyAsync($"Unable to download the image or verify the url");
                    }
                }
            }
            else
            {
                await ReplyAsync("No file or Url provided");
            }
        }
        [Command("sepia", RunMode = RunMode.Async)]
        public async Task Sepia(double amount, [Remainder] string fileUrl = "")
        {
            if(amount > 1 && Context.User.Id != 135446225565515776)
            {
                amount = 1;
            }
            if (Context.Message.Attachments.Count > 0)
            {
                fileUrl = Context.Message.Attachments.FirstOrDefault().Url;
            }
            if (fileUrl != "")
            {
                using (Context.Channel.EnterTypingState())
                {
                    try
                    {
                        
                        var photoBytes = await client.GetByteArrayAsync(fileUrl);
                        using (var stream = new MemoryStream())
                        {
                            using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(photoBytes))
                            {
                                image.Mutate(x => x.Sepia((float) amount));
                                image.Save(stream, encoder);
    
                                image.Dispose();
                            }
                            stream.Position = 0;
                            await Context.Channel.SendFileAsync(stream, $"Sepia.png");
                            await stream.FlushAsync();
                            stream.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        await ReplyAsync($"Unable to download the image or verify the url" +
                            $"\n{ex.GetType()}: {ex.Message}" +
                            $"\n{ex.StackTrace}");
                    }
                }
            }
            else
            {
                await ReplyAsync("No file or Url provided");
            }
        }
        [Command("imagetext", RunMode = RunMode.Async)]
        public async Task ImageText(string fileUrl = null, [Remainder]string text = "")
        {
            if (Context.Message.Attachments.Count > 0)
            {
                text = fileUrl + " " + text;
                fileUrl = Context.Message.Attachments.FirstOrDefault().Url;
            }
            if (fileUrl != "")
            {
                using (Context.Channel.EnterTypingState())
                {
                    try
                    {
                        
                        var photoBytes = await client.GetByteArrayAsync(fileUrl);
                        using (var stream = new MemoryStream())
                        {
                            using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(photoBytes))
                            {
                                //float padding = 10f;
                                //float textMaxWidth = image.Width - (padding * 2);
                                image.Mutate(x => x.DrawText(text, SystemFonts.CreateFont("Ubuntu", 26), SixLabors.ImageSharp.Color.Black, new PointF(20, 20)));//SystemFonts.CreateFont("Ubuntu", 26), Rgba32, new PointF(20, 20)));//, new TextGraphicsOptions(true) {WrapTextWidth = textMaxWidth,VerticalAlignment = VerticalAlignment.Top,HorizontalAlignment = HorizontalAlignment.Left });
                                image.Save(stream, encoder);
    
                                image.Dispose();
                            }
                            stream.Position = 0;
                            await Context.Channel.SendFileAsync(stream, $"ImageText.png");
                            await stream.FlushAsync();
                            stream.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        await ReplyAsync($"Unable to download the image or verify the url" +
                            $"\n{ex.GetType().ToString()}: {ex.Message}" +
                            $"\n{ex.StackTrace}");
                    }
                }
            }
            else
            {
                await ReplyAsync("No file or Url provided");
            }
        }
        [Command("pixelate", RunMode = RunMode.Async)]
        public async Task Pixelate(int size = 10, [Remainder] string fileUrl = "")
        {
            if (size > 20 && Context.User.Id != 135446225565515776)
            {
                size = 20;
            }
            if (Context.Message.Attachments.Count > 0)
            {
                fileUrl = Context.Message.Attachments.FirstOrDefault().Url;
            }
                if (fileUrl != "")
                {
                    using (Context.Channel.EnterTypingState())
                    {
                        try
                        {
                            
                            var photoBytes = await client.GetByteArrayAsync(fileUrl);
                            using (var stream = new MemoryStream())
                            {
                                using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(photoBytes))
                                {
                                    image.Mutate(x => x.Pixelate(size));
                                    image.Save(stream, encoder);
        
                                }
                                stream.Position = 0;
                                await Context.Channel.SendFileAsync(stream, $"Pixelate.png", $"Factor: `{size}`");
                                await stream.FlushAsync();
                            }
                        }
                        catch (Exception)
                        {
                            await ReplyAsync($"Unable to download the image or verify the url");
                        }
                    }
                }
                else
                {
                    await ReplyAsync("No file or Url provided");
                }
        }
        [Command("contrast", RunMode = RunMode.Async)]
        public async Task Contrast(float size = 2, [Remainder] string fileUrl = "")
        {
            if (size > 100 && Context.User.Id != 135446225565515776)
            {
                size = 100;
            }
            if (Context.Message.Attachments.Count > 0)
            {
                fileUrl = Context.Message.Attachments.FirstOrDefault()?.Url;
            }
            if (fileUrl != "")
            {
                using (Context.Channel.EnterTypingState())
                {
                    try
                    {
                        
                        var photoBytes = await client.GetByteArrayAsync(fileUrl);
                        using (var stream = new MemoryStream())
                        {
                            using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(photoBytes))
                            {
                                image.Mutate(x => x.Contrast(size));
                                image.Save(stream, encoder);
    
                            }
                            stream.Position = 0;
                            await Context.Channel.SendFileAsync(stream, $"Contrast.png", $"Factor: `{size}`");
                            await stream.FlushAsync();
                        }
                    }
                    catch (Exception)
                    {
                        await ReplyAsync($"Unable to download the image or verify the url");
                    }
                }
            }
            else
            {
                await ReplyAsync("No file or Url provided");
            }
        }
        [Command("glow", RunMode = RunMode.Async)]
        public async Task Glow(int size = 500, [Remainder] string fileUrl = "")
        {
            if (size > 2000 && Context.User.Id != 135446225565515776)
            {
                size = 2000;
            }
            if (Context.Message.Attachments.Count > 0)
            {
                fileUrl = Context.Message.Attachments.FirstOrDefault().Url;
            }
            if (fileUrl != "")
            {
                using (Context.Channel.EnterTypingState())
                {
                    try
                    {
                        
                        var photoBytes = await client.GetByteArrayAsync(fileUrl);
                        using (var stream = new MemoryStream())
                        {
                            using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(photoBytes))
                            {
                                image.Mutate(x => x.Glow(size));
                                image.Save(stream, encoder);
    
                            }
                            stream.Position = 0;
                            await Context.Channel.SendFileAsync(stream, $"Glow.png", $"Factor: `{size}`");
                            await stream.FlushAsync();
                        }
                    }
                    catch (Exception)
                    {
                        await ReplyAsync($"Unable to download the image or verify the url");
                    }
                }
            }
            else
            {
                await ReplyAsync("No file or Url provided");
            }
        }
        [Command("Skew", RunMode = RunMode.Async)]
        public async Task Skew(float x, float y, [Remainder] string fileUrl = "")
        {
            if (Context.Message.Attachments.Count > 0)
            {
                fileUrl = Context.Message.Attachments.FirstOrDefault().Url;
            }
            if (fileUrl != "")
            {
                using (Context.Channel.EnterTypingState())
                {
                    try
                    {
                        
                        var photoBytes = await client.GetByteArrayAsync(fileUrl);
                        using (var stream = new MemoryStream())
                        {
                            using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(photoBytes))
                            {
                                image.Mutate(m => m.Skew(x,y));
                                image.Save(stream, encoder);
    
                            }
                            stream.Position = 0;
                            await Context.Channel.SendFileAsync(stream, $"Skew.png", $"X: `{x}` Y: `{y}`");
                            await stream.FlushAsync();
                        }
                    }
                    catch (Exception)
                    {
                        await ReplyAsync($"Unable to download the image or verify the url");
                    }
                }
            }
            else
            {
                await ReplyAsync("No file or Url provided");
            }
        }
        [Command("Oil", RunMode = RunMode.Async)]
        public async Task Oil([Remainder] string fileUrl = "")
        {
            if (Context.Message.Attachments.Count > 0)
            {
                fileUrl = Context.Message.Attachments.FirstOrDefault().Url;
            }
            if (fileUrl != "")
            {
                using (Context.Channel.EnterTypingState())
                {
                    try
                    {
                        
                        var photoBytes = await client.GetByteArrayAsync(fileUrl);
                        using (var stream = new MemoryStream())
                        {
                            using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(photoBytes))
                            {
                                image.Mutate(x => x.OilPaint());
                                image.Save(stream, encoder);
    
                            }
                            stream.Position = 0;
                            await Context.Channel.SendFileAsync(stream, $"Oil.png");
                            await stream.FlushAsync();
                        }
                    }
                    catch (Exception)
                    {
                        await ReplyAsync($"Unable to download the image or verify the url");
                    }
                }
            }
            else
            {
                await ReplyAsync("No file or Url provided");
            }
        }
        [Command("Detectedges", RunMode = RunMode.Async)]
        public async Task DetectEdges([Remainder] string fileUrl = "")
        {
            if (Context.Message.Attachments.Count > 0)
            {
                fileUrl = Context.Message.Attachments.FirstOrDefault().Url;
            }
            if (fileUrl != "")
            {
                using (Context.Channel.EnterTypingState())
                {
                    try
                    {
                        
                        var photoBytes = await client.GetByteArrayAsync(fileUrl);
                        using (var stream = new MemoryStream())
                        {
                            using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(photoBytes))
                            {
                                image.Mutate(x => x.DetectEdges());
                                image.Save(stream, encoder);
    
                            }
                            stream.Position = 0;
                            await Context.Channel.SendFileAsync(stream, $"DetectEdges.png");
                            await stream.FlushAsync();
                        }
                    }
                    catch (Exception)
                    {
                        await ReplyAsync($"Unable to download the image or verify the url");
                    }
                }
            }
            else
            {
                await ReplyAsync("No file or Url provided");
            }
        }
        [Command("Crop", RunMode = RunMode.Async)]
        public async Task Crop(int width, int height, [Remainder] string fileUrl = "")
        {
            if (Context.Message.Attachments.Count > 0)
            {
                fileUrl = Context.Message.Attachments.FirstOrDefault().Url;
            }
            if (fileUrl != "")
            {
                using (Context.Channel.EnterTypingState())
                {
                    try
                    {
                        
                        var photoBytes = await client.GetByteArrayAsync(fileUrl);
                        using (var stream = new MemoryStream())
                        {
                            using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(photoBytes))
                            {
                                image.Mutate(x => x.Crop(width,height));
                                image.Save(stream, encoder);
    
                            }
                            stream.Position = 0;
                            await Context.Channel.SendFileAsync(stream, $"Crop.png", $"Width: `{width}` Height: `{height}`");
                            await stream.FlushAsync();
                        }
                    }
                    catch (Exception)
                    {
                        await ReplyAsync($"Unable to download the image or verify the url");
                    }
                }
            }
            else
            {
                await ReplyAsync("No file or Url provided");
            }
        }
        [Command("brightness", RunMode = RunMode.Async)]
        public async Task Brightness(float size = 2, [Remainder] string fileUrl = "")
        {
            if (size > 100 && Context.User.Id != 135446225565515776)
            {
                size = 100;
            }
            if (Context.Message.Attachments.Count > 0)
            {
                fileUrl = Context.Message.Attachments.FirstOrDefault()?.Url;
            }
            if (fileUrl != "")
            {
                using (Context.Channel.EnterTypingState())
                {
                    try
                    {
                        
                        var photoBytes = await client.GetByteArrayAsync(fileUrl);
                        using (var stream = new MemoryStream())
                        {
                            using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(photoBytes))
                            {
                                image.Mutate(x => x.Brightness(size));
                                image.Save(stream, encoder);
    
                            }
                            stream.Position = 0;
                            await Context.Channel.SendFileAsync(stream, $"Brightness.png", $"Factor: `{size}`");
                            await stream.FlushAsync();
                        }
                    }
                    catch (Exception)
                    {
                        await ReplyAsync($"Unable to download the image or verify the url");
                    }
                }
            }
            else
            {
                await ReplyAsync("No file or Url provided");
            }
        }
        [Command("rotate", RunMode = RunMode.Async)]
        public async Task Rotate(float size = 90, [Remainder] string fileUrl = "")
        {
            if (Context.Message.Attachments.Count > 0)
            {
                fileUrl = Context.Message.Attachments.FirstOrDefault().Url;
            }
            if (fileUrl != "")
            {
                using (Context.Channel.EnterTypingState())
                {
                    try
                    {
                        
                        var photoBytes = await client.GetByteArrayAsync(fileUrl);
                        using (var stream = new MemoryStream())
                        {
                            using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(photoBytes))
                            {
                                image.Mutate(x => x.Rotate(size));
                                image.Save(stream, encoder);
    
                            }
                            stream.Position = 0;
                            await Context.Channel.SendFileAsync(stream, $"Rotate.png", $"Degrees `{size}`");
                            await stream.FlushAsync();
                        }
                    }
                    catch (Exception)
                    {
                        await ReplyAsync($"Unable to download the image or verify the url");
                    }
                }
            }
            else
            {
                await ReplyAsync("No file or Url provided");
            }
        }
        [Command("saturate", RunMode = RunMode.Async)]
        public async Task Saturate(float size = 2, [Remainder] string fileUrl = "")
        {
            if (size > 100 && Context.User.Id != 135446225565515776)
            {
                size = 100;
            }
            if (Context.Message.Attachments.Count > 0)
            {
                fileUrl = Context.Message.Attachments.FirstOrDefault()?.Url;
            }
            if (fileUrl != "")
            {
                try
                {
                    
                    var photoBytes = await client.GetByteArrayAsync(fileUrl);
                    using (var stream = new MemoryStream())
                    {
                        using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(photoBytes))
                        {
                            image.Mutate(x => x.Saturate(size));
                            image.Save(stream, encoder);

                        }
                        stream.Position = 0;
                        await Context.Channel.SendFileAsync(stream, $"Saturate.png", $"Factor: `{size}`");
                        await stream.FlushAsync();
                    }
                }
                catch (Exception)
                {
                    await ReplyAsync($"Unable to download the image or verify the url");
                }
            }
            else
            {
                await ReplyAsync("No file or Url provided");
            }
        }
        [Command("blur", RunMode = RunMode.Async)]
        public async Task Blur(int size = 2, [Remainder] string fileUrl = "")
        {
            if (size > 50 && Context.User.Id != 135446225565515776)
            {
                size = 50;
            }
            if (Context.Message.Attachments.Count > 0)
            {
                fileUrl = Context.Message.Attachments.FirstOrDefault().Url;
            }
            if (fileUrl != "")
            {
                try
                {
                    
                    var photoBytes = await client.GetByteArrayAsync(fileUrl);
                    using (var stream = new MemoryStream())
                    {
                        using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(photoBytes))
                        {
                            image.Mutate(x => x.BoxBlur(size));
                            image.Save(stream, encoder);

                        }
                        stream.Position = 0;
                        await Context.Channel.SendFileAsync(stream, $"Blur.png", $"Factor: `{size}`");
                        await stream.FlushAsync();
                    }
                }
                catch (Exception)
                {
                    await ReplyAsync($"Unable to download the image or verify the url");
                }
            }
            else
            {
                await ReplyAsync("No file or Url provided");
            }
        }
        [Command("sharpen", RunMode = RunMode.Async)]
        public async Task Sharpen(int size = 2, [Remainder] string fileUrl = "")
        {
            if (size > 50 && Context.User.Id != 135446225565515776)
            {
                size = 50;
            }
            if (Context.Message.Attachments.Count > 0)
            {
                fileUrl = Context.Message.Attachments.FirstOrDefault()?.Url;
            }
            if (fileUrl != "")
            {
                using (Context.Channel.EnterTypingState())
                {
                    try
                    {
                        
                        var photoBytes = await client.GetByteArrayAsync(fileUrl);
                        using (var stream = new MemoryStream())
                        {
                            using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(photoBytes))
                            {
                                image.Mutate(x => x.GaussianSharpen(size));
                                image.Save(stream, encoder);
    
                                image.Dispose();
                            }
                            stream.Position = 0;
                            await Context.Channel.SendFileAsync(stream, $"Sharpen.png", $"Factor: `{size}`");
                            await stream.FlushAsync();
                            stream.Dispose();
                        }
                    }
                    catch (Exception)
                    {
                        await ReplyAsync($"Unable to download the image or verify the url");
                    }
                }
            }
            else
            {
                await ReplyAsync("No file or Url provided");
            }
        }
        [Command("upscale", RunMode = RunMode.Async)]
        public async Task Upscale([Remainder] string fileUrl = "")
        {
            if (Context.Message.Attachments.Count > 0)
            {
                fileUrl = Context.Message.Attachments.FirstOrDefault()?.Url;
            }
            if (fileUrl != "")
            {
                using (Context.Channel.EnterTypingState())
                {
                    try
                    {
                        int width;
                        int height;
                        
                        var photoBytes = await client.GetByteArrayAsync(fileUrl);
                        using (var stream = new MemoryStream())
                        {
                            using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(photoBytes))
                            {
                                if (image.Width > 3840 || image.Height > 2160)
                                {
                                    await ReplyAsync("Image is larger than `3840x2160`");
                                    image.Dispose();
                                }
                                else
                                {
                                    image.Mutate(x => x.Resize(image.Width * 2, image.Height * 2));
                                    image.Save(stream, encoder);
        
                                    width = image.Width;
                                    height = image.Height;
                                    image.Dispose();
                                    stream.Position = 0;
                                    await Context.Channel.SendFileAsync(stream, $"Upscale.png", $"Image upscaled by `2` (`{width}x{height}`)");
                                    await stream.FlushAsync();
                                    stream.Dispose();
                                }
                            }

                        }
                    }
                    catch (Exception)
                    {
                        await ReplyAsync($"Unable to download the image or verify the url");
                    }
                }
            }
            else
            {
                await ReplyAsync("No file or Url provided");
            }
        }
        [Command("resize", RunMode = RunMode.Async)]
        public async Task Resize(int width, int height, [Remainder] string fileUrl = "")
        {
            if (width > 3840 && Context.User.Id != 135446225565515776)
            {
                width = 3840;
            }
            if(height > 2160 && Context.User.Id != 135446225565515776)
            {
                height = 2160;
            }
            if (Context.Message.Attachments.Count > 0)
            {
                fileUrl = Context.Message.Attachments.FirstOrDefault().Url;
            }
            if (fileUrl != "")
            {
                using (Context.Channel.EnterTypingState())
                {
                    try
                    {
                        
                        var photoBytes = await client.GetByteArrayAsync(fileUrl);
                        using (var stream = new MemoryStream())
                        {
                            using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(photoBytes))
                            {
                                image.Mutate(x => x.Resize(width, height));
                                image.Save(stream, encoder);
    
                                image.Dispose();
                            }
                            stream.Position = 0;
                            await Context.Channel.SendFileAsync(stream, $"Resize.png", $"Image resized to `{width}x{height}`");
                            await stream.FlushAsync();
                            stream.Dispose();
                        }
                    }
                    catch (Exception)
                    {
                        await ReplyAsync($"Unable to download the image or verify the url");
                    }
                }
            }
            else
            {
                await ReplyAsync("No file or Url provided");
            }
        }
    }
}
