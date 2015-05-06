using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Xunit;

namespace Sep.Git.Tfs.Test.Integration
{
    //NOTE: All timestamps in these tests must specify a time zone. If they don't, the local time zone will be used in the DateTime,
    //      but the commit timestamp will use the ToUniversalTime() version of the DateTime.
    //      This will cause the hashes to differ on computers in different time zones.
    public class ListRemoteBranchesTests : IDisposable
    {
        IntegrationHelper integrationHelper;
        TextWriter output;

        public ListRemoteBranchesTests()
        {
            integrationHelper = new IntegrationHelper()
                {
                    DisableDebugMode = true
                };
            output = new StringWriter();
            Console.SetOut(output); // override console stdout, to be able to assert on git-tfs output
        }

        public void Dispose()
        {
            integrationHelper.Dispose();
        }

        [FactExceptOnUnix()]
        public void NoBranchStatesTheFact()
        {
            integrationHelper.SetupFake(r => { });

            integrationHelper.Run("list-remote-branches", integrationHelper.TfsUrl);

            AssertLine(2, "No TFS branches were found!");
        }

        public void AssertLine(int lineNum, string expectedLine)
        {
            string[] lines = Regex.Split(output.ToString(), "\r\n|\r|\n");

            Assert.True(lines.Count() >= lineNum, "There is no line " + lineNum + " in output (output has " + lines.Count() + " lines");
            Assert.Equal(expectedLine, lines[lineNum-1]);
        }
    }
}
