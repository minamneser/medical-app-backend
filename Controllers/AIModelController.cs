using Microsoft.AspNetCore.Mvc;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Collections.Generic;
using System.Linq; // Add this for FirstOrDefault

[ApiController]
[Route("api/[controller]")]
public class AIModelController : ControllerBase
{
    private readonly InferenceSession _session;
    private readonly string _inputName; // Store the input name

    public AIModelController()
    {
        // Load the ONNX model
        _session = new InferenceSession("wwwroot/model_name.onnx");

        // Get the input name dynamically
        _inputName = _session.InputMetadata.Keys.FirstOrDefault();

        // Check if an input name was found
        if (_inputName == null)
        {
            throw new Exception("No input found in the ONNX model.");
        }
    }

    [HttpPost("predict")]
    public IActionResult Predict([FromBody] float[] input)
    {
        // Prepare input tensor
        var inputTensor = new DenseTensor<float>(input, new[] { 1, 20 }); // Batch size of 1, 20 features

        // Create input for ONNX Runtime using the dynamic input name
        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor(_inputName, inputTensor)
        };

        // Run inference
        using var results = _session.Run(inputs);
        var output = results.First().AsTensor<float>().ToArray();

        // Return predictions
        return Ok(output);
    }
}