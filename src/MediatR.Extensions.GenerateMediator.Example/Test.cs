namespace MediatR.Extensions.GenerateMediator.Example
{
    [GenerateMediator]
    public partial class Test
    {
        public sealed partial record Command;

        public async Task<Example> Handler(Command request)
        {
            return await Task.FromResult(new Example());
        }
    }

    public class Example
    {

    }
}

