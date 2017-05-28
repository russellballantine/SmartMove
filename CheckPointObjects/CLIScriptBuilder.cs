﻿/********************************************************************
Copyright (c) 2017, Check Point Software Technologies Ltd.
All rights reserved.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
********************************************************************/

using System;
using System.Text;

namespace CheckPointObjects
{
    /// <summary>
    /// Generates Management API command line scripts for bash environment.
    /// </summary>
    public static class CLIScriptBuilder
    {
        public static string GenerateScriptHeader(string toolVersion, bool isObjectsScript)
        {
            string version = string.Format("# This script was generated by Check Point SmartMove tool v{0}.", toolVersion);
            string scriptType = string.Format("# Run this script on your Check Point management server to create the {0}",
                isObjectsScript ? "objects in your policy package." : "policy package.");

            var sb = new StringBuilder();
            sb.Append("#!/bin/sh")
              .Append(Environment.NewLine)
              .Append(Environment.NewLine)
              .Append(version)
              .Append(Environment.NewLine)
              .Append(scriptType)
              .Append(Environment.NewLine)
              .Append("# Note: You should run the script in \"expert mode\" as an user with root permissions.")
              .Append(Environment.NewLine);

            return sb.ToString();
        }

        public static string GenerateScriptFooter(string errorsReportFileName)
        {
            var sb = new StringBuilder();
            sb.Append(Environment.NewLine)
              .Append("mgmt_cli logout -s id.txt")
              .Append(Environment.NewLine)
              .Append(Environment.NewLine)
              .Append("if [ -f ").Append(errorsReportFileName).Append(" ]; then")
              .Append(Environment.NewLine)
              .Append("  echo ''")
              .Append(Environment.NewLine)
              .Append("  echo 'Some objects were not created successfully.'")
              .Append(Environment.NewLine)
              .Append("  echo 'Check file ").Append(errorsReportFileName).Append(" for details.'")
              .Append(Environment.NewLine)
              .Append("else")
              .Append(Environment.NewLine)
              .Append("  echo ''")
              .Append(Environment.NewLine)
              .Append("  echo 'Done. All objects were created successfully.'")
              .Append(Environment.NewLine)
              .Append("fi");

            return sb.ToString();
        }

        public static string GenerateRunCommandScript(string errorsReportFileName)
        {
            var sb = new StringBuilder();
            sb.Append("run_command() {")
              .Append(Environment.NewLine)
              .Append("  eval $cmd > last_output.txt 2>&1")
              .Append(Environment.NewLine)
              .Append("  if [ $? -ne 0 ]; then")
              .Append(Environment.NewLine)
              .Append("    echo $cmd >> ").Append(errorsReportFileName)
              .Append(Environment.NewLine)
              .Append("    cat last_output.txt >> ").Append(errorsReportFileName)
              .Append(Environment.NewLine)
              .Append("    echo ''")
              .Append(Environment.NewLine)
              .Append("    echo $cmd")
              .Append(Environment.NewLine)
              .Append("    cat last_output.txt")
              .Append(Environment.NewLine)
              .Append("  fi")
              .Append(Environment.NewLine)
              .Append("}")
              .Append(Environment.NewLine);

            return sb.ToString();
        }

        public static string GenerateLoginScript(string domainName, string errorsReportFileName)
        {
            var sb = new StringBuilder();
            sb.Append("echo 'Logging in...'")
              .Append(Environment.NewLine)
              .Append("if [ -f ").Append(errorsReportFileName).Append(" ]; then")
              .Append(Environment.NewLine)
              .Append("  rm ").Append(errorsReportFileName)
              .Append(Environment.NewLine)
              .Append("fi")
              .Append(Environment.NewLine)
              .Append("echo ''")
              .Append(Environment.NewLine);

            if (string.IsNullOrEmpty(domainName))
            {
                sb.Append("mgmt_cli login -r true -v 1.1 > id.txt").Append(Environment.NewLine);
            }
            else
            {
                sb.Append("mgmt_cli login -r true -d \"").Append(domainName).Append("\" -v 1.1 > id.txt").Append(Environment.NewLine);
            }

            sb.Append("if [ $? -ne 0 ]; then")
              .Append(Environment.NewLine)
              .Append("  echo 'Login Failed'")
              .Append(Environment.NewLine)
              .Append("  exit 1")
              .Append(Environment.NewLine)
              .Append("fi")
              .Append(Environment.NewLine);

            return sb.ToString();
        }

        public static string GeneratePublishScript()
        {
            return "mgmt_cli publish -s id.txt";
        }

        public static string GenerateObjectScript(CheckPointObject cpObject)
        {
            string scriptInstruction = cpObject.ToCLIScriptInstruction();

            var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(scriptInstruction))
            {
                sb.Append(GenerateInstructionScript(scriptInstruction)).Append(Environment.NewLine);
            }

            sb.Append("cmd='mgmt_cli ").Append(cpObject.ToCLIScript()).Append(" ignore-warnings true -s id.txt --user-agent mgmt_cli_smartmove'")
              .Append(Environment.NewLine)
              .Append("run_command");

            return sb.ToString();
        }

        public static string GenerateGeneralCommandScript(string command)
        {
            if (string.IsNullOrEmpty(command))
            {
                return "";
            }

            var sb = new StringBuilder();

            sb.Append("cmd='mgmt_cli ").Append(command).Append(" -s id.txt --user-agent mgmt_cli_smartmove'")
              .Append(Environment.NewLine)
              .Append("run_command");

            return sb.ToString();
        }

        public static string GenerateDiagnosticsCommandScript(string command)
        {
            if (string.IsNullOrEmpty(command))
            {
                return "";
            }

            var sb = new StringBuilder();

            sb.Append("mgmt_cli ").Append(command).Append(" -s id.txt > /dev/null 2>&1");

            return sb.ToString();
        }

        public static string GenerateRuleInstructionScript(string instruction)
        {
            var sb = new StringBuilder();
            sb.Append("echo -n $'\\r").Append(instruction).Append(" '");

            return sb.ToString();
        }

        public static string GenerateInstructionScript(string instruction)
        {
            var sb = new StringBuilder();
            sb.Append("echo '").Append(instruction).Append("'");

            return sb.ToString();
        }
    }
}
