# CFG2.Utils.HttpLib

An opinionated HTTP library. Those opinions include, but are not limited to:

- A HTTP lib should be *simple* to configure (just ```dotnet add package CFG2.HttpLib```)
- A HTTP lib should be *simple* to use (see example)
- A HTTP lib should default as much as possible (see opinions above)

### Usage Examples

```
namespace CFG2.Test;

using CFG2.Utils.HttpLib;

public class TestApp
{
    static void Main(string[] args)
    {
        HttpRequest req = new("https://google.com");
        Console.WriteLine(HttpLib.Get(req).Content);
    }
}
```

## Release Notes

### 1.0.0
- Initial Release