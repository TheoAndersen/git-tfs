using Sep.Git.Tfs.Vs2013;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace Sep.Git.Tfs.Test.Accept
{
    public class ConsoleOutput
    {
        public StringWriter wtr;

        public void OverrideConsoleOutput()
        {
            wtr = new StringWriter();
            Console.SetOut(wtr);
        }

        public override string ToString()
        {
            string output = wtr.ToString();
            output = output.Replace("-", "_");  // to get around strange issue, where this char stops the rest of the output from being shown in Visual Studio 2013's test explorer
            return output;
        }
    }

    public class CloneTest
    {
        public ConsoleOutput output;
        static string testDir = Path.GetTempPath() + "/testgittfs";

        public CloneTest()
        {
            output = new ConsoleOutput();
            output.OverrideConsoleOutput();

            Environment.SetEnvironmentVariable("GIT_TFS_CLIENT", "2013");
            
            if(Directory.Exists(testDir))
            {
                DeleteDirectory(testDir);
            }

            Directory.CreateDirectory(testDir);
        }

        [Fact]
        public void CanCloneARepositoryWhichHaveDeletedABranch()
        {
            /* History of $/GitTfsTFSTestServer/CreatedABranchAndDeletedIt/A
             * 
             *   Branch A                           Branch B                   Branch C (deleted)
             *   --------                           ---------                  ---------
             *   - 29003 Created folder A        
             *                                      - 29204 Created branch b
             *                                      - 29205 Changed text in B
             *                                                                 - 29206 Branched from B
             *                                                                 - 29207 deleted branch c
             *
             *   Why can't git-tfs identify that the root changeset for brach C is 29205?
             * 
             */

            Program.MainCore(new string[] 
                {
                    "clone", 
                    "https://tfs.codeplex.com:443/tfs/TFS28",
                    "$/GitTfsTFSTestServer/CreatedABranchAndDeletedIt/A",
                    testDir,
                    "--with-branches"
                });

            AssertThatOutputContainsTheWordSuccessivelyButNotError();
        }
        
        [Fact]
        public void CanCloneARepositoryWhichHaveDeletedABranchAndRenamedAnotherThingToTheSameName()
        {
            /* History of $/GitTfsTFSTestServer/CreatedDeletedAndCreatedTheSameBranch/A
             * 
             *   Branch A                           Branch B
             *   --------                           ---------
             *   - 29000 Initial folder/file        
             *   - 29001 Deleted branch A       
             *   
             *   Branch A (new branch, same name)
             *   --------
             *   - 29002 Created branch A again                       <--- this is the parent changeset of branch B
             *                                      - 29003 Branched from A
             *                                      
             *  TODO: Find out why Git-tfs thinks the default remote root is 29000 and not rightly 29002 (as it should be because 
             *  the branch created in 29000 was deleted in 29002
             */

            Program.MainCore(new string[] 
                {
                    "clone", 
                    "https://tfs.codeplex.com:443/tfs/TFS28",
                    "$/GitTfsTFSTestServer/CreatedDeletedAndCreatedTheSameBranch/A",
                    testDir,
                    "--with-branches"
                });

            AssertThatOutputContainsTheWordSuccessivelyButNotError();
        }

        [Fact]
        public void CanCloneARepositoryWhichHaveDeletedABranchAndCreatedTheSameOneAgain()
        {
            var command = new string[] 
                {
                    "clone", 
                    "https://tfs.codeplex.com:443/tfs/TFS28",
                    "$/GitTfsTFSTestServer/CreatedDeletedAndCreatedTheSameBranch/A",
                    testDir,
                    "--with-branches"
                };
            Console.WriteLine("Command: >git tfs " + string.Join(" ", command));
            Program.MainCore(command);

            AssertThatOutputContainsTheWordSuccessivelyButNotError();
        }

        [Fact]
        public void CanCloneARepositoryWithADeletedRootBranch()
        {
            Program.MainCore(new string[] 
                {
                    "clone", 
                    "https://tfs.codeplex.com:443/tfs/TFS28",
                    "$/GitTfsTFSTestServer/DeletedRootBranch/Trunk",
                    testDir,
                    "--with-branches"
                });

            AssertThatOutputContainsTheWordSuccessivelyButNotError();
        }

        private void AssertThatOutputContainsTheWordSuccessivelyButNotError()
        {
            Assert.False(output.ToString().ToLower().Contains("error"), "there shouldn't be any errors \n" + output);
            Assert.True(output.ToString().Contains("successively"), "should find success line \n" + output);
        }

        public static void DeleteDirectory(string targetDir)
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
