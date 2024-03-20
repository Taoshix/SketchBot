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
            using (Context.Channel.EnterTypingState())
            {
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
                        await Context.Interaction.FollowupWithFileAsync(stream, $"Invert.png");
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
        [SlashCommand("grayscale", "Grayscales an image")]
        [Alias("greyscale", "gray", "grey")]
        public async Task Grayscale(IAttachment inputImage)
        {
            await DeferAsync();
            using (Context.Channel.EnterTypingState())
            {
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
                        await Context.Interaction.FollowupWithFileAsync(stream, $"Grayscale.png");
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
        }
        [SlashCommand("flip", "Flips an image upside down")]
        public async Task Flip(IAttachment inputImage)
        {
            await DeferAsync();
            using (Context.Channel.EnterTypingState())
            {
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
                        await Context.Interaction.FollowupWithFileAsync(stream, $"Flip.png");
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
        [SlashCommand("sepia", "Sepia color filter")]
        public async Task Sepia(double amount, IAttachment inputImage)
        {
            await DeferAsync();
            if (amount > 1 && Context.Interaction.User.Id != 135446225565515776)
            {
                amount = 1;
            }

            using (Context.Channel.EnterTypingState())
            {
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
                        await Context.Interaction.FollowupWithFileAsync(stream, $"Sepia.png");
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
        }
        [SlashCommand("pixelate", "Pixelates an image")]
        public async Task Pixelate(int factor, IAttachment inputImage)
        {
            await DeferAsync();
            if (factor > 50 && Context.Interaction.User.Id != 135446225565515776)
            {
                factor = 50;
            }

            using (Context.Channel.EnterTypingState())
            {
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
                        await Context.Interaction.FollowupWithFileAsync(stream, $"Pixelate.png", $"Factor: `{factor}`");
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
        [SlashCommand("contrast", "Adjusts the contrast of an image")]
        public async Task Contrast(float factor, IAttachment inputImage)
        {
            await DeferAsync();
            if (factor > 100 && Context.Interaction.User.Id != 135446225565515776)
            {
                factor = 100;
            }

            using (Context.Channel.EnterTypingState())
            {
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
                        await Context.Interaction.FollowupWithFileAsync(stream, $"Contrast.png", $"Factor: `{factor}`");
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
        [SlashCommand("glow", "Glows the image")]
        public async Task Glow(int size, IAttachment inputImage)
        {
            await DeferAsync();
            if (size > 2500 && Context.Interaction.User.Id != 135446225565515776)
            {
                size = 2500;
            }
            using (Context.Channel.EnterTypingState())
            {
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
                        await Context.Interaction.FollowupWithFileAsync(stream, $"Glow.png", $"Factor: `{size}`");
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
        [SlashCommand("skew", "Skews the image")]
        public async Task Skew(float x, float y, IAttachment inputImage)
        {
            await DeferAsync();
            using (Context.Channel.EnterTypingState())
            {
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
                        await Context.Interaction.FollowupWithFileAsync(stream, $"Skew.png", $"X: `{x}` Y: `{y}`");
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
        [SlashCommand("oil", "Oil painting filter")]
        public async Task Oil(IAttachment inputImage)
        {
            await DeferAsync();
            using (Context.Channel.EnterTypingState())
            {
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
                        await Context.Interaction.FollowupWithFileAsync(stream, $"Oil.png");
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
        [SlashCommand("detectedges", "Detect edges on an image")]
        public async Task DetectEdges(IAttachment inputImage)
        {
            await DeferAsync();
            using (Context.Channel.EnterTypingState())
            {
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
                        await Context.Interaction.FollowupWithFileAsync(stream, $"DetectEdges.png");
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
        [SlashCommand("crop", "Crops the image")]
        public async Task Crop(int width, int height, IAttachment inputImage)
        {
            await DeferAsync();
            using (Context.Channel.EnterTypingState())
            {
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
                        await Context.Interaction.FollowupWithFileAsync(stream, $"Crop.png", $"Width: `{width}` Height: `{height}`");
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
        [SlashCommand("brightness", "Adjusts brightness of an image")]
        public async Task Brightness(float factor, IAttachment inputImage)
        {
            await DeferAsync();
            if (factor > 100 && Context.Interaction.User.Id != 135446225565515776)
            {
                factor = 100;
            }
            using (Context.Channel.EnterTypingState())
            {
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
                        await Context.Interaction.FollowupWithFileAsync(stream, $"Brightness.png", $"Factor: `{factor}`");
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
        [SlashCommand("rotate", "Rotates an image")]
        public async Task Rotate(float angle, IAttachment inputImage)
        {
            await DeferAsync();
            using (Context.Channel.EnterTypingState())
            {
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
                        await Context.Interaction.FollowupWithFileAsync(stream, $"Rotate.png", $"Degrees `{angle}`");
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
                    await Context.Interaction.FollowupWithFileAsync(stream, $"Saturate.png", $"Factor: `{size}`");
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
                    await Context.Interaction.FollowupWithFileAsync(stream, $"Blur.png", $"Factor: `{factor}`");
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
            using (Context.Channel.EnterTypingState())
            {
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
                        await Context.Interaction.FollowupWithFileAsync(stream, $"Sharpen.png", $"Factor: `{size}`");
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
        [SlashCommand("upscale", "Upscales an image")]
        public async Task Upscale(IAttachment inputImage)
        {
            await DeferAsync();
            using (Context.Channel.EnterTypingState())
            {
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
    
                                width = image.Width;
                                height = image.Height;
                                image.Dispose();
                                stream.Position = 0;
                                await Context.Interaction.FollowupWithFileAsync(stream, $"Upscale.png", $"Image upscaled by `2` (`{width}x{height}`)");
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
            using (Context.Channel.EnterTypingState())
            {
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
                        await Context.Interaction.FollowupWithFileAsync(stream, $"Resize.png", $"Image resized to `{width}x{height}`");
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
}
