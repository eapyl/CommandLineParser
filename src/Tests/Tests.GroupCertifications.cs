using CommandLineParser.Arguments;
using CommandLineParser.Exceptions;
using CommandLineParser.Validation;
using CommandLineParser.Exceptions;
using Xunit;

namespace Tests
{
    public partial class Tests
    {
        public class Archiver
        {
            [ValueArgument(typeof(string), ShortName = 'f', LongName = "file", Description = "Input from file")]
            public string InputFromFile;

            [ValueArgument(typeof(string), ShortName = 'u', LongName = "url", Description = "Input from url")]
            public string InputFromUrl;

            [ValueArgument(typeof(string), ShortName = 'c', LongName = "create", Description = "Create archive")]
            public string CreateArchive;

            [ValueArgument(typeof(string), ShortName = 'x', LongName = "extract", Description = "Extract archive")]
            public string ExtractArchive;

            [ValueArgument(typeof(string), ShortName = 'o', LongName = "open", Description = "Open archive")]
            public string OpenArchive;

            [SwitchArgument(ShortName = 'j', LongName = "g1a1", DefaultValue = true)]
            public bool group1Arg1;

            [SwitchArgument(ShortName = 'k', LongName = "g1a2", DefaultValue = true)]
            public bool group1Arg2;

            [SwitchArgument(ShortName = 'l', LongName = "g2a1", DefaultValue = true)]
            public bool group2Arg1;

            [SwitchArgument(ShortName = 'm', LongName = "g2a2", DefaultValue = true)]
            public bool group2Arg2;
        }

        private CommandLineParser.CommandLineParser InitGroupCertifications()
        {
            var commandLineParser = new CommandLineParser.CommandLineParser();

            Archiver a = new Archiver();

            commandLineParser.ExtractArgumentAttributes(a);

            return commandLineParser;
        }

        #region exactly one used

        [Fact]
        public void GroupCertifications_ExactlyOneUsed1()
        {
            // exactly one of the arguments x, o, c must be used
            ArgumentGroupCertification eou = new ArgumentGroupCertification("x,o,c", EArgumentGroupCondition.ExactlyOneUsed);

            var commandLineParser = InitGroupCertifications();
            commandLineParser.Certifications.Clear();
            commandLineParser.Certifications.Add(eou);

            string[] args = new[] { "-x", "file" };
            commandLineParser.ParseCommandLine(args);

            args = new[] { "-o", "file" };
            commandLineParser.ParseCommandLine(args);

            args = new[] { "-c", "file" };
            commandLineParser.ParseCommandLine(args);
        }

        [Fact]
        public void GroupCertifications_ExactlyOneUsed2()
        {
            // exactly one of the arguments x, o, c must be used
            ArgumentGroupCertification eou = new ArgumentGroupCertification("x,o,c", EArgumentGroupCondition.ExactlyOneUsed);

            var commandLineParser = InitGroupCertifications();
            commandLineParser.Certifications.Clear();
            commandLineParser.Certifications.Add(eou);

            string[] args = new[] { "-c", "file", "-x", "file2" };

            var ex = Assert.Throws<ArgumentConflictException>(() => commandLineParser.ParseCommandLine(args));
            Assert.Contains("One (and only one) of these arguments", ex.Message);
        }

        [Fact]
        public void GroupCertifications_ExactlyOneUsed3()
        {
            // exactly one of the arguments x, o, c must be used
            ArgumentGroupCertification eou = new ArgumentGroupCertification("x,o,c", EArgumentGroupCondition.ExactlyOneUsed);

            var commandLineParser = InitGroupCertifications();
            commandLineParser.Certifications.Clear();
            commandLineParser.Certifications.Add(eou);

            string[] args = new[] { "-j" };

            var ex = Assert.Throws<ArgumentConflictException>(() => commandLineParser.ParseCommandLine(args));
            Assert.Contains("One (and only one) of these arguments", ex.Message);
        }

        [Fact]
        public void GroupCertifications_ExactlyOneUsed_customError()
        {
            // exactly one of the arguments x, o, c must be used
            ArgumentGroupCertification eou = new ArgumentGroupCertification("x,o,c", EArgumentGroupCondition.ExactlyOneUsed)
            {
                Description = "my custom error"
            };
            var commandLineParser = InitGroupCertifications();
            commandLineParser.Certifications.Clear();
            commandLineParser.Certifications.Add(eou);

            string[] args = new[] { "-j" };

            var ex = Assert.Throws<ArgumentConflictException>(() => commandLineParser.ParseCommandLine(args));
            Assert.Contains("my custom error", ex.Message);
        }

        #endregion

        #region one or none used

        [Fact]
        public void GroupCertifications_OneOrNone1()
        {
            ArgumentGroupCertification oon = new ArgumentGroupCertification("f,u", EArgumentGroupCondition.OneOreNoneUsed);
            var commandLineParser = InitGroupCertifications();
            commandLineParser.Certifications.Clear();
            commandLineParser.Certifications.Add(oon);

            string[] args = new[] { "-f", "file" };
            commandLineParser.ParseCommandLine(args);

            args = new[] { "-u", "file" };
            commandLineParser.ParseCommandLine(args);
        }

        [Fact]
        public void GroupCertifications_OneOrNone2()
        {
            // exactly one of the arguments x, o, c must be used
            ArgumentGroupCertification oon = new ArgumentGroupCertification("f,u", EArgumentGroupCondition.OneOreNoneUsed);

            var commandLineParser = InitGroupCertifications();
            commandLineParser.Certifications.Clear();
            commandLineParser.Certifications.Add(oon);

            string[] args = new[] { "-x", "file" };
            commandLineParser.ParseCommandLine(args);
        }

        [Fact]
        public void GroupCertifications_OneOrNone3()
        {
            // exactly one of the arguments x, o, c must be used
            ArgumentGroupCertification oon = new ArgumentGroupCertification("f,u", EArgumentGroupCondition.OneOreNoneUsed);

            var commandLineParser = InitGroupCertifications();
            commandLineParser.Certifications.Clear();
            commandLineParser.Certifications.Add(oon);

            string[] args = new[] { "-f", "file", "-u", "file2" };

            var ex = Assert.Throws<ArgumentConflictException>(() => commandLineParser.ParseCommandLine(args));
            Assert.StartsWith("These arguments can not be used together", ex.Message);
        }

        #endregion

        #region at least one used

        [Fact]
        public void GroupCertifications_AtLeastOne1()
        {
            ArgumentGroupCertification oon = new ArgumentGroupCertification("f,u", EArgumentGroupCondition.AtLeastOneUsed);

            var commandLineParser = InitGroupCertifications();
            commandLineParser.Certifications.Clear();
            commandLineParser.Certifications.Add(oon);


            string[] args = new[] { "-f", "file" };
            commandLineParser.ParseCommandLine(args);

            args = new[] { "-u", "file" };
            commandLineParser.ParseCommandLine(args);

            args = new[] { "-u", "file", "-f", "file" };
            commandLineParser.ParseCommandLine(args);
        }

        [Fact]
        public void GroupCertifications_AtLeastOne2()
        {
            ArgumentGroupCertification oon = new ArgumentGroupCertification("f,u", EArgumentGroupCondition.AtLeastOneUsed);

            var commandLineParser = InitGroupCertifications();
            commandLineParser.Certifications.Clear();
            commandLineParser.Certifications.Add(oon);

            string[] args = new[] { "-x", "file" };

            var ex = Assert.Throws<ArgumentConflictException>(() => commandLineParser.ParseCommandLine(args));
            Assert.StartsWith("At least one of these", ex.Message);
        }

        #endregion 

        #region all or none used

        [Fact]
        public void GroupCertifications_AllOrNone1()
        {
            ArgumentGroupCertification aon = new ArgumentGroupCertification("j,k", EArgumentGroupCondition.AllOrNoneUsed);

            var commandLineParser = InitGroupCertifications();
            commandLineParser.Certifications.Clear();
            commandLineParser.Certifications.Add(aon);


            string[] args = new[] { "-f", "file" };
            commandLineParser.ParseCommandLine(args);


            args = new[] { "-j", "-k" };
            commandLineParser.ParseCommandLine(args);
        }

        [Fact]
        public void GroupCertifications_AllOrNone2()
        {
            ArgumentGroupCertification aon = new ArgumentGroupCertification("j,k", EArgumentGroupCondition.AllOrNoneUsed);

            var commandLineParser = InitGroupCertifications();
            commandLineParser.Certifications.Clear();
            commandLineParser.Certifications.Add(aon);

            string[] args = new[] { "-j", "file" };
            var ex = Assert.Throws<ArgumentConflictException>(() => commandLineParser.ParseCommandLine(args));
            Assert.StartsWith("All or none of these", ex.Message);
        }

        #endregion 

        #region all used 

        [Fact]
        public void GroupCertifications_All1()
        {
            ArgumentGroupCertification au = new ArgumentGroupCertification("j,k", EArgumentGroupCondition.AllUsed);

            var commandLineParser = InitGroupCertifications();
            commandLineParser.Certifications.Clear();
            commandLineParser.Certifications.Add(au);


            string[] args = new[] { "-j", "-k" };
            commandLineParser.ParseCommandLine(args);
        }

        [Fact]
        public void GroupCertifications_All2()
        {
            ArgumentGroupCertification au = new ArgumentGroupCertification("j,k", EArgumentGroupCondition.AllUsed);

            var commandLineParser = InitGroupCertifications();
            commandLineParser.Certifications.Clear();
            commandLineParser.Certifications.Add(au);

            string[] args = new[] { "-j", "file" };
            var ex = Assert.Throws<ArgumentConflictException>(() => commandLineParser.ParseCommandLine(args));
            Assert.StartsWith("All of these", ex.Message);
        }

        [Fact]
        public void GroupCertifications_All3()
        {
            ArgumentGroupCertification au = new ArgumentGroupCertification("j,k", EArgumentGroupCondition.AllUsed);

            var commandLineParser = InitGroupCertifications();
            commandLineParser.Certifications.Clear();
            commandLineParser.Certifications.Add(au);

            string[] args = new[] { "-x", "file" };
            var ex = Assert.Throws<ArgumentConflictException>(() => commandLineParser.ParseCommandLine(args));
            Assert.StartsWith("All of these", ex.Message);
        }

        #endregion 

        #region distinct groups

        [Fact]
        public void GroupCertifications_Distinct1()
        {
            DistinctGroupsCertification d = new DistinctGroupsCertification("j,k", "l,m");

            var commandLineParser = InitGroupCertifications();
            commandLineParser.Certifications.Clear();
            commandLineParser.Certifications.Add(d);

            string[] args = new[] { "-j", "file" };
            commandLineParser.ParseCommandLine(args);

            args = new[] { "-k", "file" };
            commandLineParser.ParseCommandLine(args);

            args = new[] { "-l", "file" };
            commandLineParser.ParseCommandLine(args);

            args = new[] { "-m", "file" };
            commandLineParser.ParseCommandLine(args);

            args = new[] { "-l", "-m" };
            commandLineParser.ParseCommandLine(args);

            args = new[] { "-j", "-k" };
            commandLineParser.ParseCommandLine(args);
        }

        [Fact]
        public void GroupCertifications_Distinct2()
        {
            DistinctGroupsCertification d = new DistinctGroupsCertification("j,k", "l,m");

            var commandLineParser = InitGroupCertifications();
            commandLineParser.Certifications.Clear();
            commandLineParser.Certifications.Add(d);

            string[] args = new[] { "-j", "-l" };
            var ex = Assert.Throws<ArgumentConflictException>(() => commandLineParser.ParseCommandLine(args));
            Assert.StartsWith("None of these", ex.Message);
        }

        [Fact]
        public void GroupCertifications_Distinct3()
        {
            DistinctGroupsCertification d = new DistinctGroupsCertification("j,k", "l,m");

            var commandLineParser = InitGroupCertifications();
            commandLineParser.Certifications.Clear();
            commandLineParser.Certifications.Add(d);

            string[] args = new[] { "-j", "-k", "-m" };
            var ex = Assert.Throws<ArgumentConflictException>(() => commandLineParser.ParseCommandLine(args));
            Assert.StartsWith("None of these", ex.Message);
        }

        [Fact]
        public void GroupCertifications_Distinct4()
        {
            DistinctGroupsCertification d = new DistinctGroupsCertification("j,k", "l,m");

            var commandLineParser = InitGroupCertifications();
            commandLineParser.Certifications.Clear();
            commandLineParser.Certifications.Add(d);

            string[] args = new[] { "-m", "-j" };
            var ex = Assert.Throws<ArgumentConflictException>(() => commandLineParser.ParseCommandLine(args));
            Assert.StartsWith("None of these", ex.Message);
        }

        #endregion

        #region argument requires other arguments 

        [Fact]
        public void ArgumentRequiresOtherArgumentsCertification_shouldPass_whenRequiredArgumentsPresent()
        {
            ArgumentRequiresOtherArgumentsCertification d = new ArgumentRequiresOtherArgumentsCertification("j", "l,m");

            var commandLineParser = InitGroupCertifications();
            commandLineParser.Certifications.Clear();
            commandLineParser.Certifications.Add(d);

            string[] args = new[] { "-j", "-l", "-m" };
            commandLineParser.ParseCommandLine(args);
            Assert.Equal(true, commandLineParser.ParsingSucceeded);
        }

        // [Fact] TODO (Stef) This test is disabled for now because no MandatoryArgumentNotSetException is thrown and I don't know if this is correct or not.
        public void ArgumentRequiresOtherArgumentsCertification_shouldFail_whenRequiredArgumentsNotPresent()
        {
            ArgumentRequiresOtherArgumentsCertification d = new ArgumentRequiresOtherArgumentsCertification("j", "l,m");

            var commandLineParser = InitGroupCertifications();
            commandLineParser.Certifications.Clear();
            commandLineParser.Certifications.Add(d);

            string[] args = new[] { "-j", "-l" };
            var ex = Assert.Throws<MandatoryArgumentNotSetException>(() => commandLineParser.ParseCommandLine(args));
            Assert.StartsWith("None of these", ex.Message);
        }
        [Fact]
        public void ArgumentRequiresOtherArgumentsCertification_shouldPass_whenMainArgumentNotPresent()
        {
            ArgumentRequiresOtherArgumentsCertification d = new ArgumentRequiresOtherArgumentsCertification("j", "l,m");

            var commandLineParser = InitGroupCertifications();
            commandLineParser.Certifications.Clear();
            commandLineParser.Certifications.Add(d);

            string[] args = new[] { "-m" };
            commandLineParser.ParseCommandLine(args);
            Assert.Equal(true, commandLineParser.ParsingSucceeded);
        }

        [DistinctGroupsCertification("s", "h,p")]
        [ArgumentRequiresOtherArgumentsCertification("h", "p,c,i,o")]
        internal class TestDefaultValues
        {
            [ValueArgument(typeof(string), ShortName = 's', LongName = "serverName", Description = "The friendly name given to the rabbit server connection.")]
            public string ServerName { get; set; }

            [ValueArgument(typeof(string), ShortName = 'h', LongName = "hostName", Description = "The fully qualified host name of the rabbit server.")]
            public string ServerHostName { get; set; }

            [ValueArgument(typeof(int), ShortName = 'p', LongName = "port", Description = "The port that should be used to connect to the rabbit server.", DefaultValue = 5672)]
            public int Port { get; set; }

            [RegexValueArgument(".*", ShortName = 'c', LongName = "credentials",
                Description =
                    "The username and password that needs to be used to connect to the rabbit server.  This needs to be in the format username|password")]
            public string Credentials { get; set; }

            [ValueArgument(typeof(string), ShortName = 'v', LongName = "virtualHost", Description = "The virtual host on the rabbit server that contains the scribe exchanges.", DefaultValue = "v.pds.ren.scribe")]
            public string ScribeVirtualHost { get; set; }

            [ValueArgument(typeof(string), ShortName = 'i', LongName = "input", Description = "The scribe input exchange.", DefaultValue = "e.pds.tools.scribe.input")]
            public string ScribeInputExchange { get; set; }

            [ValueArgument(typeof(string), ShortName = 'o', LongName = "output", Description = "The scribe output exchange.", DefaultValue = "e.pds.tools.scribe.output")]
            public string ScribeOutputExchange { get; set; }
        }

        [Fact]
        public void ArgumentRequiresOtherArgumentsCertification_shouldPass_whenDefaultValuesAreThere()
        {
            var commandLineParser = new CommandLineParser.CommandLineParser();
            var parsingTarget = new TestDefaultValues();
            commandLineParser.ExtractArgumentAttributes(parsingTarget);

            string[] args = { "-h", "dwerecrq01.hq.bn-corp.com", "-c", "renscribeui@Lithium3" };
            commandLineParser.ParseCommandLine(args);
            Assert.Equal(true, commandLineParser.ParsingSucceeded);
        }

        #endregion 
    }
}