namespace CFG2.Test;

using CFG2.Utils.AppLib;

public class TestApp
{
    static void Main(string[] args)
    {
        // This is a test application to demonstrate the use of AppLib
        Console.WriteLine("Welcome to the Test Application using AppLib!");

        // Initialize AppLib
        AppLib app = new AppLib("TestApp");

        Console.WriteLine("Test Application completed successfully.");
    }
}