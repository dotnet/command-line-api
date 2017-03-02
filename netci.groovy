// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Import the utility functionality.

import jobs.generation.Utilities;

def project = GithubProject
def branch = GithubBranchName
def isPR = true

def platformList = ['OSX:x64:Release', 'Windows_NT:x64:Release']

def static getBuildJobName(def configuration, def os, def architecture) {
    return configuration.toLowerCase() + '_' + os.toLowerCase() + '_' + architecture.toLowerCase()
}

platformList.each { platform ->
    // Calculate names
    def (os, architecture, configuration) = platform.tokenize(':')

    // Calculate job name
    def jobName = getBuildJobName(configuration, os, architecture)
    def buildCommand = '';

    // Calculate the build command
    if (os == 'Windows_NT') {
        buildCommand = ".\\build.cmd"
    }
    else {
        buildCommand = "./build.sh"
    }

    def newJob = job(Utilities.getFullJobName(project, jobName, isPR)) {
        // Set the label.
        steps {
            if (os == 'Windows_NT') {
                // Batch
                batchFile(buildCommand)
            }
            else {
                // Shell
                shell(buildCommand)
            }
        }
    }

    Utilities.setMachineAffinity(newJob, os, 'latest-or-auto')
    Utilities.standardJobSetup(newJob, project, isPR, "*/${branch}")
    Utilities.addXUnitDotNETResults(newJob, '**/*-testResults.xml')
    Utilities.addGithubPRTriggerForBranch(newJob, branch, "${os} ${architecture} ${configuration} Build")
}


