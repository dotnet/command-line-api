// Groovy Script: http://www.groovy-lang.org/syntax.html
// Jenkins DSL: https://github.com/jenkinsci/job-dsl-plugin/wiki

import jobs.generation.ArchivalSettings;
import jobs.generation.Utilities;
import jobs.generation.InternalUtilities;

static getJobName(def os, def config) {
  return "${os}_${config}"
}

['Windows_NT', 'Ubuntu16.04'].each { os ->
  ['Debug', 'Release'].each { config ->
    [true, false].each { isPR ->
      def project = GithubProject
      def branch = GithubBranchName
      def jobName = getJobName(config, os)
      def buildCommand = '';
      def machineAffinity = ''

      if (os == 'Windows_NT') {
        buildCommand = ".\\eng\\common\\cibuild.cmd -configuration $config"
        machineAffinity = 'win2016-base'
      } else {
        buildCommand = "./eng/common/cibuild.sh --configuration $config"
        machineAffinity = 'latest-or-auto'
      }

      def newJob = job(Utilities.getFullJobName(project, jobName, isPR)) {
        steps {
          if (os == 'Windows_NT') {
            batchFile(buildCommand)
          }
          else {
            shell(buildCommand)
          }
        }
      }

      InternalUtilities.standardJobSetup(newJob, project, isPR, "*/${branch}")
      Utilities.setMachineAffinity(newJob, os, machineAffinity)

      if (isPR) {
        Utilities.addGithubPRTriggerForBranch(newJob, branch, "$os $config")
      }

      Utilities.addXUnitDotNETResults(newJob, "artifacts/$config/TestResults/*.xml", false)

      def archiveSettings = new ArchivalSettings()
      archiveSettings.addFiles("artifacts/$config/log/*")
      archiveSettings.addFiles("artifacts/$config/TestResults/*")
      archiveSettings.setFailIfNothingArchived()
      archiveSettings.setArchiveOnFailure()
      Utilities.addArchival(newJob, archiveSettings)
    }
  }
}
