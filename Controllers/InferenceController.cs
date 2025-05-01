using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting; // Add this
using SixLabors.ImageSharp.Formats.Png; //Ensure you have Png format imported

namespace ModelTest.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InferenceController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly InferenceSession _session;
        private static string _modelInputName;

        public InferenceController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;

            // Load the ONNX model
            string modelPath = Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot", "EEGmodelV2.onnx"); // Place your ONNX model in wwwroot
            _session = new InferenceSession(modelPath);

            // Get the input name dynamically
            _modelInputName = _session.InputMetadata.Keys.FirstOrDefault();
            if (_modelInputName == null)
            {
                throw new Exception("No input found in the ONNX model.");
            }
        }

        [HttpPost("predictImage")]
        public IActionResult PredictImage(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return BadRequest("No image file uploaded.");
            }

            // Save the image to a temporary file
            var tempFilePath = Path.GetTempFileName() + ".png";
            using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                imageFile.CopyTo(stream);
            }

            // Preprocess the image
            DenseTensor<float> inputTensor;
            try
            {
                inputTensor = ImagePreprocessor.ProcessSpecFromImage(tempFilePath);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Image processing error: {ex.Message}");
            }

            // Make the prediction
            string predictionLabel;
            try
            {
                predictionLabel = Predict(inputTensor);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Prediction error: {ex.Message}");
            }

            // Clean up the temporary file
            System.IO.File.Delete(tempFilePath);

            return Ok(predictionLabel);
        }

        // The input of this method should be TFTensor instead of float array
        private string Predict(DenseTensor<float> inputTensor)
        {
            // Setup inputs and outputs
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor(_modelInputName, inputTensor)
            }.AsReadOnly();

            // Run the model
            using var results = _session.Run(inputs);

            // Postprocess to get label
            var vector = results.First().AsTensor<float>().ToArray();
            int pred_idx = Array.IndexOf(vector, vector.Max());

            string label = GetLabelFromIndex(pred_idx);
            return label;
        }
        //Create new method to convert the index to label
        private string GetLabelFromIndex(int index)
        {
            Dictionary<string, int> labels = new Dictionary<string, int>
            {
                { "Seizure", 0 },
                { "GPD", 1 },
                { "LRDA", 2 },
                { "Other", 3 },
                { "GRDA", 4 },
                { "LPD", 5 }
            };

            foreach (var entry in labels)
            {
                if (entry.Value == index)
                {
                    return entry.Key;
                }
            }

            return "Unknown"; // Handle the case where the index doesn't match any label
        }
    }
}