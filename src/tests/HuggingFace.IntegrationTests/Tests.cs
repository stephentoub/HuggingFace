using HuggingFace;

namespace tryAGI.OpenAI.IntegrationTests;

[TestClass]
public class GeneralTests
{
    [TestMethod]
    public async Task Generate()
    {
        var apiKey =
            Environment.GetEnvironmentVariable("HUGGINGFACE_API_KEY") ??
            throw new AssertInconclusiveException("HUGGINGFACE_API_KEY environment variable is not found.");

        using var client = new HttpClient();
        var api = new HuggingFaceApi(apiKey, client);
        var response = await api.GenerateTextAsync(
            RecommendedModelIds.Gpt2,
            new GenerateTextRequest
            {
                Inputs = "Hello",
                Parameters = new GenerateTextRequestParameters
                {
                    Max_new_tokens = 250,
                    Return_full_text = false,
                },
                Options = new GenerateTextRequestOptions
                {
                    Use_cache = true,
                    Wait_for_model = false,
                },
            });
        response.Should().NotBeEmpty();

        foreach (var value in response)
        {
            Console.WriteLine(value.Generated_text);
        }
    }
    
    [TestMethod]
    public async Task GenerateError()
    {
        var apiKey =
            Environment.GetEnvironmentVariable("HUGGINGFACE_API_KEY") ??
            throw new AssertInconclusiveException("HUGGINGFACE_API_KEY environment variable is not found.");

        using var client = new HttpClient();
        var api = new HuggingFaceApi(apiKey, client);
        Func<Task> act = async () => await api.GenerateTextAsync(
            RecommendedModelIds.Gpt2,
            new GenerateTextRequest
            {
                Inputs = "Hello",
                Parameters = new GenerateTextRequestParameters
                {
                    Max_new_tokens = 2501,
                    Return_full_text = false,
                },
                Options = new GenerateTextRequestOptions
                {
                    Use_cache = true,
                    Wait_for_model = false,
                },
            });
        
        (await act.Should().ThrowAsync<ApiException<ErrorResponse>>())
            .Which.Result.Error.First().Should().Be("Error in `parameters.max_new_tokens`: ensure this value is less than or equal to 250");
    }
}
