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

        [FactExceptOnUnix()]
        public void ASingleRootBranch()
        {
            integrationHelper.SetupFake(r => 
            {
                r.SetRootBranch("$/MyProject/RootBranch");
            });

            integrationHelper.Run("list-remote-branches", integrationHelper.TfsUrl);

            AssertLine(2, "TFS branches that could be cloned:");
            AssertLine(3, "");
            AssertLine(4, " $/MyProject/RootBranch [*]");
            AssertLine(5, "");
        }

        [FactExceptOnUnix()]
        public void ASingleRootBranchWithTwoSubBranches()
        {
            integrationHelper.SetupFake(r =>
            {
                r.SetRootBranch("$/MyProject/RootBranch");
                r.Changeset(1, "initial commit", DateTime.Parse("2012-01-01 12:12:12 -05:00"));
                r.BranchChangeset(2, "branched out", DateTime.Parse("2012-01-01 12:14:12 -05:00"), "$/MyProject/RootBranch", "$/MyProject/SubBranchA", 1);
                r.BranchChangeset(3, "branched out", DateTime.Parse("2012-01-01 12:15:12 -05:00"), "$/MyProject/RootBranch", "$/MyProject/SubBranchB", 1);
            });

            integrationHelper.Run("list-remote-branches", integrationHelper.TfsUrl);

            AssertLine(2, "TFS branches that could be cloned:");
            AssertLine(3, "");
            AssertLine(4, " $/MyProject/RootBranch [*]");
            AssertLine(5, " | ");
            AssertLine(6, " +- $/MyProject/SubBranchA");
            AssertLine(7, " | ");
            AssertLine(8, " +- $/MyProject/SubBranchB");
            AssertLine(9, "");
        }

        [FactExceptOnUnix()]
        public void ASingleDeletedRootBranchWithTwoSubBranches()
        {
            integrationHelper.SetupFake(r =>
            {
                r.SetRootBranch("$/MyProject/RootBranch");
                r.SetBranchDeleted("$/MyProject/RootBranch");
                r.Changeset(1, "initial commit", DateTime.Parse("2012-01-01 12:12:12 -05:00"));
                r.BranchChangeset(2, "branched out", DateTime.Parse("2012-01-01 12:14:12 -05:00"), "$/MyProject/RootBranch", "$/MyProject/SubBranchA", 1);
                r.BranchChangeset(3, "branched out", DateTime.Parse("2012-01-01 12:15:12 -05:00"), "$/MyProject/RootBranch", "$/MyProject/SubBranchB", 1);
            });

            integrationHelper.Run("list-remote-branches", integrationHelper.TfsUrl);

            AssertLine(2, "TFS branches that could be cloned:");
            AssertLine(3, "");
            AssertLine(4, " $/MyProject/RootBranch [*]");
            AssertLine(5, " | ");
            AssertLine(6, " +- $/MyProject/SubBranchA");
            AssertLine(7, " | ");
            AssertLine(8, " +- $/MyProject/SubBranchB");
            AssertLine(9, "");
        }

        public void AssertLine(int lineNum, string expectedLine)
        {
            string[] lines = Regex.Split(output.ToString(), "\r\n|\r|\n");

            Assert.True(lines.Count() >= lineNum, "There is no line " + lineNum + " in output (output has " + lines.Count() + " lines");
            AssertEqual(expectedLine, lines[lineNum-1], "Line " + lineNum);
        }

        // Assert Equal with message - XUnit thinks these isn't needed, but they are wrong :(
        public void AssertEqual(string expected, string actual, string message)
        {
            try
            {
                Assert.Equal(expected, actual);
            }
            catch(Exception e)
            {
                Assert.True(false, message + "\r\n" + e.Message);
            }
        }
    }
}
