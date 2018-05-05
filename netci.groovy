// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Import the utility functionality.

import jobs.generation.ArchivalSettings;
import jobs.generation.Utilities;

def project = GithubProject
def branch = GithubBranchName

def static getBuildJobName(def configuration, def os, def architecture) {
    return configuration.toLowerCase() + '_' + os.toLowerCase() + '_' + architecture.toLowerCase()
}

['OSX10.12', 'Ubuntu16.04', 'Windows_NT'].each { os ->
    ['x64'].each { architecture ->
        ['Debug', 'Release'].each { config ->
            [true, false].each { isPR ->
                // Calculate job name
                def jobName = getBuildJobName(config, os, architecture)
                def buildCommand = '';

                def osBase = os
                def machineAffinity = 'latest-or-auto'

                def newJob = job(Utilities.getFullJobName(project, jobName, isPR)) {
                    // Set the label.
                    steps {
                        if (osBase == 'Windows_NT') {
                            // Batch
                            batchFile(".\\build\\cibuild.cmd -configuration $config")
                        }
                        else {
                            // Shell
                            shell("./build/cibuild.sh --configuration $config")
                        }
                    }
                }

                Utilities.setMachineAffinity(newJob, osBase, machineAffinity)
                Utilities.standardJobSetup(newJob, project, isPR, "*/${branch}")

                if (isPR) {
                    Utilities.addGithubPRTriggerForBranch(newJob, branch, "$os $architecture $config")
                }

                def archiveSettings = new ArchivalSettings()
                archiveSettings.addFiles("artifacts/$config/log/*")
                archiveSettings.addFiles("artifacts/$config/TestResults/*")
                archiveSettings.setFailIfNothingArchived()
                archiveSettings.setArchiveOnFailure()
                Utilities.addArchival(newJob, archiveSettings)
            }
        }
    }
}
