﻿using System;
using System.Linq;
using NUnit.Framework;
using PRISM;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
namespace PRISMTest
{
    [TestFixture]
    class CommandLineParserTests
    {
        private const bool showHelpOnError = false;
        private const bool outputErrors = false;

        [Test]
        public void TestBadKey1()
        {
            var parser = new CommandLineParser<BadKey1>();
            var result = parser.ParseArgs(new[] {"-bad-name", "b"}, showHelpOnError, outputErrors);
            Assert.IsFalse(result.Success, "Parser did not fail with '-' at start of arg key");
            Assert.IsTrue(result.ParseErrors.Any(x => x.Contains("bad character") && x.Contains("char '-'")), "Error message does not contain \"bad character\" and \"char '-'\"");
        }

        [Test]
        public void TestBadKey2()
        {
            var parser = new CommandLineParser<BadKey2>();
            var result = parser.ParseArgs(new[] { "/bad/name", "b" }, showHelpOnError, outputErrors);
            Assert.IsFalse(result.Success, "Parser did not fail with '/' at start of arg key");
            Assert.IsTrue(result.ParseErrors.Any(x => x.Contains("bad character") && x.Contains("char '/'")), "Error message does not contain \"bad character\" and \"char '/'\"");
        }

        [Test]
        public void TestBadKey3()
        {
            var parser = new CommandLineParser<BadKey3>();
            var result = parser.ParseArgs(new[] { "-badname", "b" }, showHelpOnError, outputErrors);
            Assert.IsFalse(result.Success, "Parser did not fail with duplicate arg keys");
            Assert.IsTrue(result.ParseErrors.Any(x => x.ToLower().Contains("duplicate") && x.Contains("badname")), "Error message does not contain \"duplicate\" and \"badname\"");
        }

        [Test]
        public void TestBadKey4()
        {
            var parser = new CommandLineParser<BadKey4>();
            var result = parser.ParseArgs(new[] { "-NoGood", "b" }, showHelpOnError, outputErrors);
            Assert.IsFalse(result.Success, "Parser did not fail with duplicate arg keys");
            Assert.IsTrue(result.ParseErrors.Any(x => x.ToLower().Contains("duplicate") && x.Contains("NoGood")), "Error message does not contain \"duplicate\" and \"NoGood\"");
        }
        [Test]
        public void TestBadKey5()
        {
            var parser = new CommandLineParser<BadKey5>();
            var result = parser.ParseArgs(new[] { "-bad name", "b" }, showHelpOnError, outputErrors);
            Assert.IsFalse(result.Success, "Parser did not fail with ' ' in arg key");
            Assert.IsTrue(result.ParseErrors.Any(x => x.Contains("bad character") && x.Contains("char ' '")), "Error message does not contain \"bad character\" and \"char ' '\"");
        }

        [Test]
        public void TestBadKey6()
        {
            var parser = new CommandLineParser<BadKey6>();
            var result = parser.ParseArgs(new[] { "/bad:name", "b" }, showHelpOnError, outputErrors);
            Assert.IsFalse(result.Success, "Parser did not fail with ':' in arg key");
            Assert.IsTrue(result.ParseErrors.Any(x => x.Contains("bad character") && x.Contains("char ':'")), "Error message does not contain \"bad character\" and \"char ':'\"");
        }

        [Test]
        public void TestBadKey7()
        {
            var parser = new CommandLineParser<BadKey7>();
            var result = parser.ParseArgs(new[] { "-bad=name", "b" }, showHelpOnError, outputErrors);
            Assert.IsFalse(result.Success, "Parser did not fail with '=' in arg key");
            Assert.IsTrue(result.ParseErrors.Any(x => x.Contains("bad character") && x.Contains("char '='")), "Error message does not contain \"bad character\" and \"char '='\"");
        }

        private class BadKey1
        {
            [Option("-bad-name")]
            public string BadName { get; set; }
        }

        private class BadKey2
        {
            [Option("/bad/name")]
            public string BadName { get; set; }
        }

        private class BadKey3
        {
            [Option("badname")]
            public string BadName { get; set; }

            [Option("badname")]
            public string BadName2 { get; set; }
        }

        private class BadKey4
        {
            [Option("g")]
            public string GoodName { get; set; }

            [Option("G")]
            public string GoodNameUCase { get; set; }

            [Option("NoGood")]
            public string TheBadName { get; set; }

            [Option("NoGood")]
            public string TheBadName2 { get; set; }
        }

        private class BadKey5
        {
            [Option("bad name")]
            public string BadName { get; set; }
        }

        private class BadKey6
        {
            [Option("bad:name")]
            public string BadName { get; set; }
        }

        private class BadKey7
        {
            [Option("bad=name")]
            public string BadName { get; set; }
        }

        [Test]
        public void TestOkayKey1()
        {
            var parser = new CommandLineParser<OkayKey1>();
            var result = parser.ParseArgs(new[] { "-okay-name", "b" }, showHelpOnError, outputErrors);
            Assert.IsTrue(result.Success, "Parser failed with '-' not at start of arg key");
            Assert.IsTrue(result.ParseErrors.Count == 0, "Error list not empty");
        }

        [Test]
        public void TestOkayKey2()
        {
            var parser = new CommandLineParser<OkayKey2>();
            var result = parser.ParseArgs(new[] { "/okay/name", "b" }, showHelpOnError, outputErrors);
            Assert.IsTrue(result.Success, "Parser failed with '/' not at start of arg key");
            Assert.IsTrue(result.ParseErrors.Count == 0, "Error list not empty");
        }

        [Test]
        public void TestHelpKey1()
        {
            var parser = new CommandLineParser<OkayKey2>();
            var result = parser.ParseArgs(new[] { "--help" }, showHelpOnError, outputErrors);
            Assert.IsFalse(result.Success, "Parser did not \"fail\" when user requested the help screen");
            Assert.IsTrue(result.ParseErrors.Count == 0, "Error list not empty");
        }

        [Test]
        public void TestHelpKey2()
        {
            var parser = new CommandLineParser<OkayKey2> {
                ParamFlagCharacters = new[] {'/', '-'}
            };
            var result = parser.ParseArgs(new[] { "/?" }, showHelpOnError, outputErrors);
            Assert.IsFalse(result.Success, "Parser did not \"fail\" when user requested the help screen");
            Assert.IsTrue(result.ParseErrors.Count == 0, "Error list not empty");
        }

        private class OkayKey1
        {
            [Option("okay-name")]
            public string OkayName { get; set; }
        }

        private class OkayKey2
        {
            [Option("okay/name", HelpText = "This switch has a slash in the name; that's unusual, but allowed")]
            public string OkayName { get; set; }

            [Option("verbose", "wordy", "detailed",
                HelpText = "Use this switch to include verbose output, in homage to which this help text includes lorem ipsum dolor sit amet, " +
                           "elit phasellus, penatibus sed eget quis suspendisse. Quam suspendisse accumsan in vestibulum, ante donec dolor nibh, " +
                           "mauris sodales, orci mollis et convallis felis porta. Felis eu, metus sed, a quam nulla commodo nulla sit, diam sed morbi " +
                           "ut euismod et, diam vestibulum cursus. Dolor sit scelerisque tellus, wisi nec, mauris etiam potenti laoreet non, " +
                           "leo aliquam nonummy. Pulvinar tortor, leo rutrum blandit velit, quis lacus.")]
            public string Verbose { get; set; }

            /// <summary>
            /// Note that ## should be updated at runtime by calling UpdatePropertyHelpText
            /// </summary>
            [Option("smooth", "alternativeLongNameForSmooth", HelpText = "Number of points to smooth; default is ## points")]
            public int Smooth { get; set; }

            [Option("smooth2", "alternativeLongNameForSmooth2", HelpText = "Number of points to smooth", DefaultValueFormatString = "; default is {0} points")]
            public int Smooth2 { get; set; }

            [Option("gnat", HelpText = "I'm a supported argument, but I don't get advertised.", Hidden = true)]
            public int NoSeeUm { get; set; }
        }

        [Test]
        public void TestGood()
        {
            var args = new[]
            {
                "MyInputFile.txt",
                "-minInt", "11",
                "-maxInt:5",
                "/minMaxInt", "2",
                "/minDbl:15",
                "-maxDbl", "5.5",
                "-minmaxdbl", "2.4",
                "-g", @"C:\Users\User",
                @"/G:C:\Users\User2\",
                "-over", "This string should be overwritten",
                "-ab", "TestAb1",
                "-aB", "TestAb2",
                "RandomlyPlacedOutputFile.txt",
                "-Ab", "TestAb3",
                "-AB=TestAb4",
                "-b1", "true",
                "-b2", "False",
                "/b3",
                "-1",
                "-over", "This string should be used",
                "-strArray", "value1",
                "-strArray", "value2",
                "-strArray", "value3",
                "UnusedPositionalArg.txt",
                "-intArray", "0",
                "-intArray", "1",
                "-intArray", "2",
                "-intArray", "3",
                "-intArray", "4",
                "-dblArray", "1.0"
            };
            var parser = new CommandLineParser<ArgsVariety>();
            var result = parser.ParseArgs(args, showHelpOnError, outputErrors);
            var options = result.ParsedResults;
            Assert.IsTrue(result.Success, "Parser failed to parse valid args");
            Assert.IsTrue(result.ParseErrors.Count == 0, "Error list not empty");
            Assert.AreEqual(11, options.IntMinOnly);
            Assert.AreEqual(5, options.IntMaxOnly);
            Assert.AreEqual(2, options.IntMinMax);
            Assert.AreEqual(15, options.DblMinOnly);
            Assert.AreEqual(5.5, options.DblMaxOnly);
            Assert.AreEqual(2.4, options.DblMinMax);
            Assert.AreEqual(@"C:\Users\User", options.LowerChar);
            Assert.AreEqual(@"C:\Users\User2\", options.UpperChar);
            Assert.AreEqual("TestAb1", options.Ab1);
            Assert.AreEqual("TestAb2", options.Ab2);
            Assert.AreEqual("TestAb3", options.Ab3);
            Assert.AreEqual("TestAb4", options.Ab4);
            Assert.AreEqual(true, options.BoolCheck1);
            Assert.AreEqual(false, options.BoolCheck2);
            Assert.AreEqual(true, options.BoolCheck3);
            Assert.AreEqual("MyInputFile.txt", options.InputFilePath);
            Assert.AreEqual("RandomlyPlacedOutputFile.txt", options.OutputFilePath);
            Assert.AreEqual(true, options.NumericArg);
            Assert.AreEqual("This string should be used", options.Overrides);
            Assert.AreEqual(3, options.StringArray.Length);
            Assert.AreEqual("value1", options.StringArray[0]);
            Assert.AreEqual("value2", options.StringArray[1]);
            Assert.AreEqual("value3", options.StringArray[2]);
            Assert.AreEqual(5, options.IntArray.Length);
            Assert.AreEqual(0, options.IntArray[0]);
            Assert.AreEqual(1, options.IntArray[1]);
            Assert.AreEqual(2, options.IntArray[2]);
            Assert.AreEqual(3, options.IntArray[3]);
            Assert.AreEqual(4, options.IntArray[4]);
            Assert.AreEqual(1, options.DblArray.Length);
            Assert.AreEqual(1.0, options.DblArray[0]);
        }

        [Test]
        public void TestPositionalArgs()
        {
            var args = new[]
            {
                "MyInputFile.txt",
                "OutputFile.txt",
                "UnusedPositionalArg.txt",
            };

            var parser = new CommandLineParser<ArgsPositionalOnly>();
            var result = parser.ParseArgs(args, showHelpOnError, outputErrors);
            var options = result.ParsedResults;

            Assert.IsTrue(result.Success, "Parser failed to parse valid args");
            Assert.IsTrue(result.ParseErrors.Count == 0, "Error list not empty");

            Assert.AreEqual("MyInputFile.txt", options.InputFilePath);
            Assert.AreEqual("OutputFile.txt", options.OutputFilePath);
        }

        [Test]
        public void TestMinInt1()
        {
            var args = new[]
            {
                "-minInt", "5",
            };
            var parser = new CommandLineParser<ArgsVariety>();
            var result = parser.ParseArgs(args, showHelpOnError, outputErrors);
            Assert.IsFalse(result.Success, "Parser did not fail on value less than min");
            Assert.IsTrue(result.ParseErrors.Any(x => x.ToLower().Contains("minint") && x.Contains("is less than minimum")), "Error message does not contain \"minInt\" and \"is less than minimum\"");
        }

        [Test]
        public void TestMinInt2()
        {
            var args = new[]
            {
                "-minMaxInt", "-15",
            };
            var parser = new CommandLineParser<ArgsVariety>();
            var result = parser.ParseArgs(args, showHelpOnError, outputErrors);
            Assert.IsFalse(result.Success, "Parser did not fail on value less than min");
            Assert.IsTrue(result.ParseErrors.Any(x => x.ToLower().Contains("minmaxint") && x.Contains("is less than minimum")), "Error message does not contain \"minMaxInt\" and \"is less than minimum\"");
        }

        [Test]
        public void TestMinInt3()
        {
            var args = new[]
            {
                "-minIntBad", "15",
            };
            var parser = new CommandLineParser<ArgsVariety>();
            var result = parser.ParseArgs(args, showHelpOnError, outputErrors);
            Assert.IsFalse(result.Success, "Parser did not fail on invalid min type");
            Assert.IsTrue(result.ParseErrors.Any(x => x.ToLower().Contains("minintbad") && x.Contains("cannot cast min or max to type")), "Error message does not contain \"minIntBad\" and \"cannot cast min or max to type\"");
        }

        [Test]
        public void TestBadMinInt()
        {
            var args = new[]
            {
                "-minInt", "15.0",
            };
            var parser = new CommandLineParser<ArgsVariety>();
            var result = parser.ParseArgs(args, showHelpOnError, outputErrors);
            Assert.IsFalse(result.Success, "Parser did not on invalid type");
            Assert.IsTrue(result.ParseErrors.Any(x => x.ToLower().Contains("minint") && x.Contains("cannot cast") && x.Contains("to type")), "Error message does not contain \"minInt\", \"cannot cast\", and \"to type\"");
        }

        [Test]
        public void TestMaxInt1()
        {
            var args = new[]
            {
                "-maxInt", "15",
            };
            var parser = new CommandLineParser<ArgsVariety>();
            var result = parser.ParseArgs(args, showHelpOnError, outputErrors);
            Assert.IsFalse(result.Success, "Parser did not fail on value greater than max");
            Assert.IsTrue(result.ParseErrors.Any(x => x.ToLower().Contains("maxint") && x.Contains("is greater than maximum")), "Error message does not contain \"maxInt\" and \"is greater than maximum\"");
        }

        [Test]
        public void TestMaxInt2()
        {
            var args = new[]
            {
                "-maxIntBad", "5",
            };
            var parser = new CommandLineParser<ArgsVariety>();
            var result = parser.ParseArgs(args, showHelpOnError, outputErrors);
            Assert.IsFalse(result.Success, "Parser did not fail on invalid max type");
            Assert.IsTrue(result.ParseErrors.Any(x => x.ToLower().Contains("maxintbad") && x.Contains("cannot cast min or max to type")), "Error message does not contain \"maxIntBad\" and \"cannot cast min or max to type\"");
        }

        [Test]
        public void TestBadMaxInt()
        {
            var args = new[]
            {
                "-maxInt", "9.0",
            };
            var parser = new CommandLineParser<ArgsVariety>();
            var result = parser.ParseArgs(args, showHelpOnError, outputErrors);
            Assert.IsFalse(result.Success, "Parser did not on invalid type");
            Assert.IsTrue(result.ParseErrors.Any(x => x.ToLower().Contains("maxint") && x.Contains("cannot cast") && x.Contains("to type")), "Error message does not contain \"maxInt\", \"cannot cast\", and \"to type\"");
        }

        [Test]
        public void TestMinDbl1()
        {
            var args = new[]
            {
                "-minDbl", "5",
            };
            var parser = new CommandLineParser<ArgsVariety>();
            var result = parser.ParseArgs(args, showHelpOnError, outputErrors);
            Assert.IsFalse(result.Success, "Parser did not fail on value less than min");
            Assert.IsTrue(result.ParseErrors.Any(x => x.ToLower().Contains("mindbl") && x.Contains("is less than minimum")), "Error message does not contain \"minDbl\" and \"is less than minimum\"");
        }

        [Test]
        public void TestMinDbl2()
        {
            var args = new[]
            {
                "-minMaxDbl", "-15",
            };
            var parser = new CommandLineParser<ArgsVariety>();
            var result = parser.ParseArgs(args, showHelpOnError, outputErrors);
            Assert.IsFalse(result.Success, "Parser did not fail on value less than min");
            Assert.IsTrue(result.ParseErrors.Any(x => x.ToLower().Contains("minmaxdbl") && x.Contains("is less than minimum")), "Error message does not contain \"minMaxDbl\" and \"is less than minimum\"");
        }

        [Test]
        public void TestMinDbl3()
        {
            var args = new[]
            {
                "-minDblBad", "15",
            };
            var parser = new CommandLineParser<ArgsVariety>();
            var result = parser.ParseArgs(args, showHelpOnError, outputErrors);
            Assert.IsFalse(result.Success, "Parser did not fail on invalid min type");
            Assert.IsTrue(result.ParseErrors.Any(x => x.ToLower().Contains("mindblbad") && x.Contains("cannot cast min or max to type")), "Error message does not contain \"minDblBad\" and \"cannot cast min or max to type\"");
        }

        [Test]
        public void TestBadMinDbl()
        {
            var args = new[]
            {
                "-minDbl", "15n",
            };
            var parser = new CommandLineParser<ArgsVariety>();
            var result = parser.ParseArgs(args, showHelpOnError, outputErrors);
            Assert.IsFalse(result.Success, "Parser did not fail on invalid type");
            Assert.IsTrue(result.ParseErrors.Any(x => x.ToLower().Contains("mindbl") && x.Contains("cannot cast") && x.Contains("to type")), "Error message does not contain \"minDbl\", \"cannot cast\", and \"to type\"");
        }

        [Test]
        public void TestMaxDbl1()
        {
            var args = new[]
            {
                "-maxDbl", "15",
            };
            var parser = new CommandLineParser<ArgsVariety>();
            var result = parser.ParseArgs(args, showHelpOnError, outputErrors);
            Assert.IsFalse(result.Success, "Parser did not fail on value greater than max");
            Assert.IsTrue(result.ParseErrors.Any(x => x.ToLower().Contains("maxdbl") && x.Contains("is greater than maximum")), "Error message does not contain \"maxDbl\" and \"is greater than maximum\"");
        }

        [Test]
        public void TestMaxDbl2()
        {
            var args = new[]
            {
                "-maxDblBad", "5",
            };
            var parser = new CommandLineParser<ArgsVariety>();
            var result = parser.ParseArgs(args, showHelpOnError, outputErrors);
            Assert.IsFalse(result.Success, "Parser did not fail on invalid max type");
            Assert.IsTrue(result.ParseErrors.Any(x => x.ToLower().Contains("maxdblbad") && x.Contains("cannot cast min or max to type")), "Error message does not contain \"maxDblBad\" and \"cannot cast min or max to type\"");
        }

        [Test]
        public void TestBadMaxDbl()
        {
            var args = new[]
            {
                "-maxDbl", "5t",
            };
            var parser = new CommandLineParser<ArgsVariety>();
            var result = parser.ParseArgs(args, showHelpOnError, outputErrors);
            Assert.IsFalse(result.Success, "Parser did not fail on invalid type");
            Assert.IsTrue(result.ParseErrors.Any(x => x.ToLower().Contains("maxdbl") && x.Contains("cannot cast") && x.Contains("to type")), "Error message does not contain \"maxDbl\", \"cannot cast\", and \"to type\"");
        }

        // ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
        // ReSharper disable MemberCanBePrivate.Local
        private class ArgsEnum
        {
            [Option("u", HelpText = "I Am Unknown")]
            public TestEnum BeUnknown { get; set; }

            [Option("2t", HelpText = "I Am Too True")]
            public TestEnum TooTrue { get; set; }

            [Option("l", HelpText = "I AM LEGEND!")]
            public TestEnum Legendary { get; set; }

            [Option("f", HelpText = "I lied.")]
            public TestEnum TooBad { get; set; }

            [Option("result", HelpText = "How bad will it be?")]
            public TestEnumFlags ResultEffect { get; set; }

            public ArgsEnum()
            {
                BeUnknown = TestEnum.Unknown;
                TooTrue = TestEnum.CantBeTruer;
                Legendary = TestEnum.Legend;
                TooBad = TestEnum.False;
                ResultEffect = TestEnumFlags.Good;
            }
        }

        private enum TestEnum
        {
            Unknown = 0,
            True = 1,
            DoublyTrue = 2,
            CantBeTruer = 3,
            False = -1,
            Legend = 100
        }

        [Flags]
        private enum TestEnumFlags
        {
            [System.ComponentModel.Description("It's Okay")]
            Good = 0x0,
            [System.ComponentModel.Description("It's Bad")]
            Bad = 0x1,
            [System.ComponentModel.Description("It's really bad")]
            Ugly = 0x2,
            [System.ComponentModel.Description("It's the end of the world")]
            Apocalypse = 0x4,
            [System.ComponentModel.Description("You just blew up the universe")]
            EndOfUniverse = 0x8
        }

        [Test]
        public void TestEnumHelp()
        {
            var parser = new CommandLineParser<ArgsEnum>();
            parser.PrintHelp();
        }

        [Test]
        public void TestEnumArgs()
        {
            var args = new[]
            {
                "-u", "doublytrue",
                "-f", "100",
                "-result", "14"
            };
            var parser = new CommandLineParser<ArgsEnum>();
            var result = parser.ParseArgs(args, showHelpOnError, outputErrors);
            var options = result.ParsedResults;
            Assert.IsTrue(result.Success, "Parser failed to parse valid args");
            Assert.IsTrue(result.ParseErrors.Count == 0, "Error list not empty");
            Assert.AreEqual(TestEnum.DoublyTrue, options.BeUnknown);
            Assert.AreEqual(TestEnum.Legend, options.TooBad);
            Assert.AreEqual("Ugly, Apocalypse, EndOfUniverse", options.ResultEffect.ToString());
            Assert.AreEqual(TestEnumFlags.Ugly | TestEnumFlags.Apocalypse | TestEnumFlags.EndOfUniverse, options.ResultEffect);
        }

        [Test]
        public void TestEnumArgsBadArg()
        {
            var args = new[]
            {
                "-u", "DoublyTrue",
                "-f", "100",
                "-2t", "5"
            };
            var parser = new CommandLineParser<ArgsEnum>();
            var result = parser.ParseArgs(args, showHelpOnError, outputErrors);
            Assert.IsFalse(result.Success, "Parser did not on invalid type");
            Assert.IsTrue(result.ParseErrors.Any(x => x.ToLower().Contains("2t") && x.Contains("cannot cast") && x.Contains("to type")), "Error message does not contain \"2t\", \"cannot cast\", and \"to type\"");
        }

        [Test]
        public void TestEnumArgsBadArgString()
        {
            var args = new[]
            {
                "-u", "doublytrue",
                "-f", "100",
                "-2t", "Legendary"
            };
            var parser = new CommandLineParser<ArgsEnum>();
            var result = parser.ParseArgs(args, showHelpOnError, outputErrors);
            Assert.IsFalse(result.Success, "Parser did not on invalid type");
            Assert.IsTrue(result.ParseErrors.Any(x => x.ToLower().Contains("2t") && x.Contains("cannot cast") && x.Contains("to type")), "Error message does not contain \"2t\", \"cannot cast\", and \"to type\"");
        }

        [Test]
        public void TestPrintHelp()
        {
            var exeName = "Test.exe";

            var parser = new CommandLineParser<OkayKey2>(){
                ProgramInfo = "This program sed tempor urna. Proin porta scelerisque nisi, " +
                              "non vestibulum elit varius vel. Sed sed tristique orci, sit amet " +
                              "feugiat risus. \n\n" +
                              "Vivamus ac fermentum eros. Aliquam accumsan est vitae quam rhoncus, " +
                              "et consectetur ante egestas. Donec in enim id arcu mollis sagittis. " +
                              "Nulla venenatis tellus at urna feugiat, et placerat tortor dapibus. " +
                              "Proin in bibendum dui. Phasellus bibendum purus non mi semper, vel rhoncus " +
                              "massa viverra. Aenean quis neque sit amet nisi posuere congue. \n\n" +
                              "Options for EnumTypeMode are:\n" +
                              "  0 for feugiat risu\n" +
                              "  1 for porttitor libero\n" +
                              "  2 for sapien maximus varius\n" +
                              "  3 for lorem luctus\n" +
                              "  4 for pulvinar quam at libero dapibus\n" +
                              "  5 for tortor loborti\n" +
                              "  6 for ante nec nisi consequat\n" +
                              "  7 for facilisis vestibulum risus",

                ContactInfo = "Program written by Maecenas cursus for fermentum ullamcorper velit in 2017" +
                              Environment.NewLine +
                              "E-mail: person@place.org or alternate@place.org" + Environment.NewLine +
                              "Website: http://panomics.pnnl.gov/ or http://omics.pnl.gov or http://www.sysbio.org/resources/staff/",

                UsageExamples = {
                    exeName + " InputFile.txt",
                    exeName + " InputFile.txt /Start:2",
                    exeName + " InputFile.txt /Start:2 /EnumTypeMode:2 /Smooth:7"
                }
            };

            parser.PrintHelp();
        }

        [Test]
        public void TestUpdateHelpText()
        {
            var exeName = "Test.exe";

            var parser = new CommandLineParser<OkayKey2>()
            {
                ProgramInfo = "This program does some work",

                ContactInfo = "Program written by an actual human",

                UsageExamples = {
                    exeName + " InputFile.txt",
                    exeName + " InputFile.txt /Start:2",
                    exeName + " InputFile.txt /Start:2 /EnumTypeMode:2 /Smooth:7"
                }
            };

            parser.PrintHelp();

            parser.UpdatePropertyHelpText("Smooth", "##", "15");
            parser.PrintHelp();

            parser.UpdatePropertyHelpText("Smooth", "New help text");
            parser.PrintHelp();
        }

        private class ArgsVariety
        {
            [Option("minInt", Min = 10)]
            public int IntMinOnly { get; set; }

            [Option("maxInt", Max = 10)]
            public int IntMaxOnly { get; set; }

            [Option("minmaxInt", Min = -5, Max = 5)]
            public int IntMinMax { get; set; }

            [Option("minIntBad", Min = 10.1)]
            public int IntMinBad { get; set; }

            [Option("maxIntBad", Max = 10.5)]
            public int IntMaxBad { get; set; }

            [Option("minDbl", Min = 10)]
            public double DblMinOnly { get; set; }

            [Option("maxDbl", Max = 10)]
            public double DblMaxOnly { get; set; }

            [Option("minmaxDbl", Min = -5, Max = 5)]
            public double DblMinMax { get; set; }

            [Option("minDblBad", Min = "bad")]
            public double DblMinBad { get; set; }

            [Option("maxDblBad", Max = "bad")]
            public double DblMaxBad { get; set; }

            [Option("g")]
            public string LowerChar { get; set; }

            [Option("G")]
            public string UpperChar { get; set; }

            [Option("ab")]
            public string Ab1 { get; set; }

            [Option("aB")]
            public string Ab2 { get; set; }

            [Option("Ab")]
            public string Ab3 { get; set; }

            [Option("AB")]
            public string Ab4 { get; set; }

            [Option("b1")]
            public bool BoolCheck1 { get; set; }

            [Option("b2")]
            public bool BoolCheck2 { get; set; }

            [Option("b3")]
            public bool BoolCheck3 { get; set; }

            [Option("i", ArgPosition = 1)]
            public string InputFilePath { get; set; }

            [Option("1")]
            public bool NumericArg { get; set; }

            [Option("o", ArgPosition = 2)]
            public string OutputFilePath { get; set; }

            [Option("over")]
            public string Overrides { get; set; }

            [Option("strArray")]
            public string[] StringArray { get; set; }

            [Option("intArray")]
            public int[] IntArray { get; set; }

            [Option("dblArray")]
            public double[] DblArray { get; set; }
        }


        private class ArgsPositionalOnly
        {

            [Option("i", ArgPosition = 1)]
            public string InputFilePath { get; set; }

            [Option("o", ArgPosition = 2)]
            public string OutputFilePath { get; set; }
        }

        [Test]
        public void TestArgExistsPropertyFail1()
        {
            var parser = new CommandLineParser<ArgExistsPropertyFail1>();
            var result = parser.ParseArgs(new[] { "-L", "bad" }, showHelpOnError, outputErrors);
            Assert.IsFalse(result.Success, "Parser did not fail with empty or whitespace ArgExistsProperty");
            Assert.IsTrue(result.ParseErrors.Any(x => x.Contains(nameof(OptionAttribute.ArgExistsProperty)) && x.Contains("null") && x.Contains("boolean")), $"Error message does not contain \"{nameof(OptionAttribute.ArgExistsProperty)}\", \"null\", and \"boolean\"");
        }

        [Test]
        public void TestArgExistsPropertyFail2()
        {
            var parser = new CommandLineParser<ArgExistsPropertyFail2>();
            var result = parser.ParseArgs(new[] { "-L", "bad" }, showHelpOnError, outputErrors);
            Assert.IsFalse(result.Success, "Parser did not fail with an ArgExistsProperty non-existent property");
            Assert.IsTrue(result.ParseErrors.Any(x => x.Contains(nameof(OptionAttribute.ArgExistsProperty)) && x.Contains("not exist") && x.Contains("not a boolean")), $"Error message does not contain \"{nameof(OptionAttribute.ArgExistsProperty)}\", \"not exist\", and \"not a boolean\"");
        }

        [Test]
        public void TestArgExistsPropertyFail3()
        {
            var parser = new CommandLineParser<ArgExistsPropertyFail3>();
            var result = parser.ParseArgs(new[] { "-L", "bad" }, showHelpOnError, outputErrors);
            Assert.IsFalse(result.Success, "Parser did not fail with an ArgExistsProperty non-boolean property");
            Assert.IsTrue(result.ParseErrors.Any(x => x.Contains(nameof(OptionAttribute.ArgExistsProperty)) && x.Contains("not exist") && x.Contains("not a boolean")), $"Error message does not contain \"{nameof(OptionAttribute.ArgExistsProperty)}\", \"not exist\", and \"not a boolean\"");
        }

        [Test]
        public void TestArgExistsPropertyGood1()
        {
            var parser = new CommandLineParser<ArgExistsPropertyGood>();
            var result = parser.ParseArgs(new[] { "-L" }, showHelpOnError, outputErrors);
            Assert.IsTrue(result.Success, "Parser failed to process with valid and specified ArgExistsProperty");
            var defaults = new ArgExistsPropertyGood();
            var options = result.ParsedResults;
            Assert.AreEqual(true, options.LogEnabled, "LogEnabled should be true!!");
            Assert.AreEqual(defaults.LogFilePath, options.LogFilePath, "LogFilePath should match the default value!!");
        }

        [Test]
        public void TestArgExistsPropertyGood2()
        {
            var logFileName = "myLogFile.txt";
            var parser = new CommandLineParser<ArgExistsPropertyGood>();
            var result = parser.ParseArgs(new[] { "-L", logFileName }, showHelpOnError, outputErrors);
            Assert.IsTrue(result.Success, "Parser failed to process with valid and specified ArgExistsProperty");
            var defaults = new ArgExistsPropertyGood();
            var options = result.ParsedResults;
            Assert.AreEqual(true, options.LogEnabled, "LogEnabled should be true!!");
            Assert.AreEqual(logFileName, options.LogFilePath, "LogFilePath should match the provided value!!");
        }

        private class ArgExistsPropertyFail1
        {
            public ArgExistsPropertyFail1()
            {
                LogEnabled = false;
                LogFilePath = "log.txt";
            }

            public bool LogEnabled { get; set; }

            [Option("log", "L", HelpText = "If specified, write to a log file. Can optionally provide a log file path", ArgExistsProperty = " ")]
            public string LogFilePath { get; set; }
        }

        private class ArgExistsPropertyFail2
        {
            public ArgExistsPropertyFail2()
            {
                LogEnabled = false;
                LogFilePath = "log.txt";
            }

            public bool LogEnabled { get; set; }

            [Option("log", "L", HelpText = "If specified, write to a log file. Can optionally provide a log file path", ArgExistsProperty = "LogEnabled1")]
            public string LogFilePath { get; set; }
        }

        private class ArgExistsPropertyFail3
        {
            public ArgExistsPropertyFail3()
            {
                LogEnabled = 0;
                LogFilePath = "log.txt";
            }

            public int LogEnabled { get; set; }

            [Option("log", "L", HelpText = "If specified, write to a log file. Can optionally provide a log file path", ArgExistsProperty = "LogEnabled1")]
            public string LogFilePath { get; set; }
        }

        private class ArgExistsPropertyGood
        {
            public ArgExistsPropertyGood()
            {
                LogEnabled = false;
                LogFilePath = "log.txt";
            }

            public bool LogEnabled { get; set; }

            [Option("log", "L", HelpText = "If specified, write to a log file. Can optionally provide a log file path", ArgExistsProperty = nameof(LogEnabled))]
            public string LogFilePath { get; set; }
        }
    }
}
