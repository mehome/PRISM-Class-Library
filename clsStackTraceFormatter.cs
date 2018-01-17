using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PRISM
{
    /// <summary>
    /// This class produces an easier-to read stack trace for an exception
    /// See the descriptions for functions GetExceptionStackTrace and
    /// GetExceptionStackTraceMultiLine for example text
    /// </summary>
    /// <remarks></remarks>
    public class clsStackTraceFormatter
    {
        /// <summary>
        /// Stack trace label string
        /// </summary>
        public const string STACK_TRACE_TITLE = "Stack trace: ";

        /// <summary>
        /// String interpolated between parts of the stack trace
        /// </summary>
        public const string STACK_CHAIN_SEPARATOR = "-:-";

        /// <summary>
        /// Prefix added before the final file is listed in the stacktrace
        /// </summary>
        public const string FINAL_FILE_PREFIX = " in ";

        /// <summary>
        /// Parses the StackTrace text of the given exception to return a compact description of the current stack
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <param name="includeInnerExceptionMessages">When true, also append details of any inner exceptions</param>
        /// <returns>
        /// String of the form:
        /// "Stack trace: clsCodeTest.Test-:-clsCodeTest.TestException-:-clsCodeTest.InnerTestException in clsCodeTest.vb:line 86"
        /// </returns>
        /// <remarks>Useful for removing the full file paths included in the default stack trace</remarks>
        public static string GetExceptionStackTrace(Exception ex, bool includeInnerExceptionMessages = true)
        {

            var stackTraceData = GetExceptionStackTraceData(ex).ToList();

            var sbStackTrace = new StringBuilder();
            for (var index = 0; index <= stackTraceData.Count - 1; index++)
            {
                if (index == stackTraceData.Count - 1 && stackTraceData[index].StartsWith(FINAL_FILE_PREFIX))
                {
                    sbStackTrace.Append(stackTraceData[index]);
                    break;
                }

                if (index == 0)
                {
                    sbStackTrace.Append(STACK_TRACE_TITLE + stackTraceData[index]);
                }
                else
                {
                    sbStackTrace.Append(STACK_CHAIN_SEPARATOR + stackTraceData[index]);
                }
            }

            if (!includeInnerExceptionMessages)
                return sbStackTrace.ToString();

            var innerException = ex.InnerException;
            while (innerException != null)
            {
                sbStackTrace.Append(STACK_CHAIN_SEPARATOR + innerException.Message);
                innerException = innerException.InnerException;
            }

            return sbStackTrace.ToString();

        }

        /// <summary>
        /// Parses the StackTrace text of the given exception to return a cleaned up description of the current stack,
        /// with one line for each function in the call tree
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <param name="includeInnerExceptionMessages">When true, also append details of any inner exceptions</param>
        /// <returns>
        /// Stack trace:
        ///   clsCodeTest.Test
        ///   clsCodeTest.TestException
        ///   clsCodeTest.InnerTestException
        ///    in clsCodeTest.vb:line 86
        /// </returns>
        /// <remarks>Useful for removing the full file paths included in the default stack trace</remarks>
        public static string GetExceptionStackTraceMultiLine(Exception ex, bool includeInnerExceptionMessages = true)
        {

            var stackTraceData = GetExceptionStackTraceData(ex);

            var sbStackTrace = new StringBuilder();
            sbStackTrace.AppendLine(STACK_TRACE_TITLE);

            foreach (var traceItem in stackTraceData)
            {
                sbStackTrace.AppendLine("  " + traceItem);
            }

            if (!includeInnerExceptionMessages)
                return sbStackTrace.ToString();

            var innerException = ex.InnerException;
            while (innerException != null)
            {
                sbStackTrace.AppendLine();
                sbStackTrace.AppendLine(innerException.Message);
                innerException = innerException.InnerException;
            }

            return sbStackTrace.ToString();

        }

        /// <summary>
        /// Parses the StackTrace text of the given exception to return a cleaned up description of the current stack
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <returns>
        /// List of function names; for example:
        ///   clsCodeTest.Test
        ///   clsCodeTest.TestException
        ///   clsCodeTest.InnerTestException
        ///    in clsCodeTest.vb:line 86
        /// </returns>
        /// <remarks></remarks>
        public static IEnumerable<string> GetExceptionStackTraceData(Exception ex)
        {
            return GetExceptionStackTraceData(ex.StackTrace);
        }

        /// <summary>
        /// Parses the given StackTrace text to return a cleaned up description of the current stack
        /// </summary>
        /// <param name="stackTraceText">Exception.StackTrace data</param>
        /// <returns>
        /// List of function names; for example:
        ///   clsCodeTest.Test
        ///   clsCodeTest.TestException
        ///   clsCodeTest.InnerTestException
        ///    in clsCodeTest.vb:line 86
        /// </returns>
        /// <remarks></remarks>
        public static IEnumerable<string> GetExceptionStackTraceData(string stackTraceText)
        {
            const string REGEX_FUNCTION_NAME = @"at ([^(]+)\(";
            const string REGEX_FILE_NAME = @"in .+\\(.+)";

            const string CODE_LINE_PREFIX = ":line ";
            const string REGEX_LINE_IN_CODE = CODE_LINE_PREFIX + "\\d+";

            var lstFunctions = new List<string>();
            var finalFile = string.Empty;

            var reFunctionName = new Regex(REGEX_FUNCTION_NAME, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var reFileName = new Regex(REGEX_FILE_NAME, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var reLineInCode = new Regex(REGEX_LINE_IN_CODE, RegexOptions.Compiled | RegexOptions.IgnoreCase);

            if (string.IsNullOrWhiteSpace(stackTraceText))
            {
                var emptyStackTrace = new List<string> {
                    "Empty stack trace"
                };
                return emptyStackTrace;
            }

            // Process each line in objException.StackTrace
            // Populate strFunctions() with the function name of each line
            using (var reader = new StringReader(stackTraceText))
            {

                while (reader.Peek() > -1)
                {
                    var dataLine = reader.ReadLine();

                    if (string.IsNullOrEmpty(dataLine))
                        continue;

                    var currentFunction = string.Empty;

                    var functionMatch = reFunctionName.Match(dataLine);
                    var lineMatch = reLineInCode.Match(dataLine);

                    // Also extract the file name where the Exception occurred
                    var fileMatch = reFileName.Match(dataLine);
                    string currentFunctionFile;

                    if (fileMatch.Success)
                    {
                        currentFunctionFile = fileMatch.Groups[1].Value;
                        if (finalFile.Length == 0)
                        {
                            var lineMatchFinalFile = reLineInCode.Match(currentFunctionFile);
                            if (lineMatchFinalFile.Success)
                            {
                                finalFile = currentFunctionFile.Substring(0, lineMatchFinalFile.Index);
                            }
                            else
                            {
                                finalFile = currentFunctionFile;
                            }
                        }
                    }
                    else
                    {
                        currentFunctionFile = string.Empty;
                    }

                    if (functionMatch.Success)
                    {
                        currentFunction = functionMatch.Groups[1].Value;
                    }
                    else
                    {
                        // Look for the word " in "
                        var charIndex = dataLine.ToLower().IndexOf(" in ", StringComparison.Ordinal);
                        if (charIndex == 0)
                        {
                            // " in" not found; look for the first space after startIndex 4
                            charIndex = dataLine.IndexOf(" ", 4, StringComparison.Ordinal);
                        }

                        if (charIndex == 0)
                        {
                            // Space not found; use the entire string
                            charIndex = dataLine.Length - 1;
                        }

                        if (charIndex > 0)
                        {
                            currentFunction = dataLine.Substring(0, charIndex);
                        }

                    }

                    var functionDescription = currentFunction;

                    if (!string.IsNullOrEmpty(currentFunctionFile))
                    {
                        if (string.IsNullOrEmpty(finalFile) ||
                            !TrimLinePrefix(finalFile, CODE_LINE_PREFIX).Equals(
                                TrimLinePrefix(currentFunctionFile, CODE_LINE_PREFIX), StringComparison.OrdinalIgnoreCase))
                        {
                            functionDescription += FINAL_FILE_PREFIX + currentFunctionFile;
                        }
                    }

                    if (lineMatch.Success && !functionDescription.Contains(CODE_LINE_PREFIX))
                    {
                        functionDescription += lineMatch.Value;
                    }

                    lstFunctions.Add(functionDescription);
                }

            }

            var stackTraceData = new List<string>();
            stackTraceData.AddRange(lstFunctions);
            stackTraceData.Reverse();

            if (!string.IsNullOrWhiteSpace(finalFile))
            {
                stackTraceData.Add(FINAL_FILE_PREFIX + finalFile);
            }

            return stackTraceData;

        }

        private static string TrimLinePrefix(string fileDescription, string codeLinePrefix)
        {

            var matchIndex = fileDescription.IndexOf(codeLinePrefix, StringComparison.Ordinal);
            if (matchIndex > 0)
            {
                return fileDescription.Substring(0, matchIndex);
            }

            return fileDescription;
        }

    }
}
