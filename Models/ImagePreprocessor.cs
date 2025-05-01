using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp.Formats.Png;

public static class ImagePreprocessor
{
    public static DenseTensor<float> ProcessSpecFromImage(string imagePath)
    {
        // Load the image using ImageSharp (specify format to ensure correct decoding)
        using var image = Image.Load<L8>(imagePath);

        // Resize to 300x400
        image.Mutate(x => x.Resize(width: 400, height: 300, sampler: KnownResamplers.Lanczos3));

        // Convert the image to a grayscale float array
        float[] floatArray = new float[image.Width * image.Height];
        int i = 0;
        for (int y = 0; y < image.Height; y++) //ImageSharp uses x,y coordinates, so keep it for here and transpose it after in array.
        {
            for (int x = 0; x < image.Width; x++)
            {
                floatArray[i++] = image[x, y].PackedValue / 255f; // Normalize to 0-1 range
            }
        }

        // Transpose the float array from 300,400 => 400,300
        float[] transposedArray = new float[image.Width * image.Height];
        i = 0;
        for (int x = 0; x < image.Height; x++)
        {
            for (int y = 0; y < image.Width; y++)
            {
                transposedArray[i] = floatArray[y * image.Height + x];
                i++;
            }
        }

        // Normalization and Convert the single channel to 3 channels

        float[] normalizedAndTiledArray = NormalizeAndTileArray(transposedArray, image.Width, image.Height);

        // Create a DenseTensor from the normalized array
        var tensor = new DenseTensor<float>(normalizedAndTiledArray, new[] { 1, 400, 300, 3 });//shape [1, 400, 300, 3]. 
        return tensor;
    }

    private static float[] NormalizeAndTileArray(float[] grayScaleArray, int width, int height)
    {
        //Clip values to avoid inf and NaN, and convert the values by log function.
        float[] clippedArray = new float[width * height];
        for (int i = 0; i < grayScaleArray.Length; i++)
        {
            clippedArray[i] = (float)Math.Max(Math.Min(grayScaleArray[i], 8.0f), Math.Exp(-4.0f));
            clippedArray[i] = MathF.Log(clippedArray[i]);
        }

        //Calculate mean and stddev of the array
        float mean = clippedArray.Average();
        float stdDev = CalculateStdDev(clippedArray, mean);

        //Normalize the array with calculated mean and stddev
        float[] normalizedArray = new float[width * height];
        for (int i = 0; i < clippedArray.Length; i++)
        {
            normalizedArray[i] = (clippedArray[i] - mean) / (stdDev + 1e-6f);
        }

        //Tile 3 channels with the normalized Array
        float[] result = new float[width * height * 3];
        for (int i = 0; i < width * height; i++)
        {
            result[3 * i] = normalizedArray[i];
            result[3 * i + 1] = normalizedArray[i];
            result[3 * i + 2] = normalizedArray[i];
        }

        return result;
    }

    private static float CalculateStdDev(float[] values, float mean)
    {
        double sumOfSquares = 0.0;
        foreach (float value in values)
        {
            sumOfSquares += Math.Pow(value - mean, 2);
        }
        return (float)Math.Sqrt(sumOfSquares / values.Length);
    }
}