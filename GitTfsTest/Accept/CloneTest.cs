using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace Sep.Git.Tfs.Test.Accept
{
    public class CloneTest
    {
        StringWriter wtr;
        string testDir = Path.GetTempPath() + "/testgittfs";

        public void SetFixture(CloneTest data)
        {
            wtr = new StringWriter();
            Console.SetOut(wtr);
            Environment.SetEnvironmentVariable("GIT_TFS_CLIENT", "2013");
            
            if(Directory.Exists(testDir))
            {
                DeleteDirectory(testDir);
            }

            Directory.CreateDirectory(testDir);
        }

        [Fact]
        public void CanCloneARepositoryWhichHaveDeletedABranchAndRenamedAnotherThingToTheSameName()
        {
            //??? this just works??
            SetFixture(this);
            Program.MainCore(new string[] 
                {
                    "clone", 
                    "https://tfs.codeplex.com:443/tfs/TFS28",
                    "$/GitTfsTFSTestServer/CreatedDeletedAndCreatedTheSameBranch/A",
                    testDir,
                    "--with-branches"
                });
            string output = wtr.ToString();
            Assert.False(output.ToLower().Contains("error"), "there shouldn't be any errors \n" + output);
            Assert.True(output.Contains("successively"), "should find success line \n" + output);
        }

        [Fact]
        public void CanCloneARepositoryWhichHaveDeletedABranchAndCreatedTheSameOneAgain()
        {
            //??? this just works??
            SetFixture(this);
            Program.MainCore(new string[] 
                {
                    "clone", 
                    "https://tfs.codeplex.com:443/tfs/TFS28",
                    "$/GitTfsTFSTestServer/CreatedDeletedAndCreatedTheSameBranch/A",
                    testDir,
                    "--with-branches"
                });
            string output = wtr.ToString();
            Assert.False(output.ToLower().Contains("error"), "there shouldn't be any errors \n" + output);
            Assert.True(output.Contains("successively"), "should find success line \n" + output);
        }

        [Fact]
        public void CanCloneARepositoryWithADeletedRootBranch()
        {
            SetFixture(this);
            Program.MainCore(new string[] 
                {
                    "clone", 
                    "https://tfs.codeplex.com:443/tfs/TFS28",
                    "$/GitTfsTFSTestServer/DeletedRootBranch/Trunk",
                    testDir,
                    "--with-branches"
                });
            string output = wtr.ToString();
            Assert.False(output.ToLower().Contains("error"), "there shouldn't be any errors \n" + output);
            Assert.True(output.Contains("successively"), "should find success line \n" + output);
        }

        public void DeleteDirectory(string targetDir)
        {
            File.SetAttributes(targetDir, FileAttributes.Normal);

            string[] files = Directory.GetFiles(targetDir);
            string[] dirs = Directory.GetDirectories(targetDir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(targetDir, false);
        }


    }
}
