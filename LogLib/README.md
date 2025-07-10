## CFG2.Utils.LogLib

An opinionated logger. Those opinions include, but are not limited to:

- A logger should be *simple* to configure (just ```dotnet add package CFG2.LogLib```)
- A logger should be *simple* to use (see example)
- A logger should default as much as possible (see opinions above)

### Usage Examples

*Recommended* Usage
```
using CFG2.Utils.LogLib;

public class Program
{
    private static readonly Logger logger = Logger.Instance(@"C:\My\App");

    static void Main(string[] args)
    {
        logger.Warn("Hello");
    }
}
```

Using Trace without having an instance of a Logger
```
using CFG2.Utils.LogLib;

public class Program
{
    static void Main(string[] args)
    {
        Logger.Trace("Hello");
    }
}
```

## Release Notes

### 1.0.0
- Initial Release