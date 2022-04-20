using System;

public class CompilerException : Exception
{
    public int lineNumber { get; set; }

    public CompilerException()
    {
    }

    public CompilerException(string message, int lineNumber)
        : base(message)
    {
        this.lineNumber = lineNumber;
    }

    public CompilerException(string message, Exception inner)
        : base(message, inner)
    {
    }
}