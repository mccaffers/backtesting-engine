using System.Collections;

namespace Utilities;

public class ReportEnvironmentVariables
{

    public static string[] ignoreEnvWhenReporting =
    {
        "CLOUD_ID",
        "ELASTIC_PASSWORD",
        "ELASTIC_USER",
        "CLEAN_LOCAL_TICK_FOLDER",
        "CONSOLE_LOG",
        "FASTER_PROCESSING_BY_SKIPPING_SOME_DATA",
        "LAMBDA_LOG",
        "PULL_FROM_AWS_S3",
        "REPORT_INDIVIDUAL_TRADES",
        "REPORT_TO_ELASTICSEARCH",
        "SYMBOL_FOLDER",
        "SYSTEM_LOG",
        "SCALING_FACTOR",
        "ENVIRONMENT",
        "payload"
    };

    public static string[] ignoreList = {
                                        "_MSBUILDTLENABLED",
                                        "VSCODE_GIT_ASKPASS_EXTRA_ARGS",
                                        "SUDO_USER",
                                        "XDG_DATA_DIRS",
                                        "SUDO_COMMAND",
                                        "LESSCLOSE",
                                        "MSBUILDFAILONDRIVEENUMERATINGWILDCARD",
                                        "SUDO_UID",
                                        "original_dir",
                                        "EFC_8864",
                                        "MANPATH",
                                        "VSCODE_INJECTION",
                                        "DOTNET_HOST_PATH",
                                        "HOMEBREW_PREFIX",
                                        "MSBuildExtensionsPath",
                                        "MSBUILDENSURESTDOUTFORTASKPROCESSES",
                                        "HOMEBREW_CELLAR",
                                        "VSCODE_GIT_IPC_HANDLE",
                                        "COLORTERM",
                                        "GIT_ASKPASS",
                                        "TERM_PROGRAM_VERSION",
                                        "HOME",
                                        "__CFBundleIdentifier",
                                        "TERM_PROGRAM",
                                        "_",
                                        "TERM",
                                        "MallocNanoZone",
                                        "COMMAND_MODE",
                                        "PWD",
                                        "ZDOTDIR",
                                        "HOMEBREW_REPOSITORY",
                                        "ORIGINAL_XDG_CURRENT_DESKTOP",
                                        "VSCODE_GIT_ASKPASS_MAIN",
                                        "MSBuildLoadMicrosoftTargetsReadOnly",
                                        "SHLVL",
                                        "XPC_SERVICE_NAME",
                                        "USER_ZDOTDIR",
                                        "LANG",
                                        "MSBuildSDKsPath",
                                        "MSBUILDUSESERVER",
                                        "VSTEST_WINAPPHOST_DOTNET_ROOT",
                                        "LOGNAME",
                                        "XPC_FLAGS",
                                        "TMPDIR",
                                        "OLDPWD",
                                        "SHELL",
                                        "USER",
                                        "VSCODE_GIT_ASKPASS_NODE",
                                        "INFOPATH",
                                        "PATH",
                                        "__CF_USER_TEXT_ENCODING",
                                        "DOTNET_CLI_TELEMETRY_SESSIONID",
                                        "SSH_AUTH_SOCK",
                                        "GPG_TTY",
                                        "AWS_ACCESS_KEY_ID",
                                        "AWS_SECRET_ACCESS_KEY",
                                        "AWS_SESSION_TOKEN",
                                        "FPS_BROWSER_USER_PROFILE_STRING",
                                        "FPS_BROWSER_APP_PROFILE_STRING",
                                        "EFC_102348",
                                        "EFC_10580",
                                        "DOTNET_ROOT_ARM64",
                                        "GIT_VERSION_VENDORED",
                                        "lib_git",
                                        "LOCALAPPDATA",
                                        "clink_architecture",
                                        "CMDER_INIT_START",
                                        "ComSpec",
                                        "CMDER_CONFIG_DIR",
                                        "CMDER_ALIASES",
                                        "position",
                                        "ConEmuDir",
                                        "VENDORED_BUILD",
                                        "TEMP",
                                        "lib_path",
                                        "print_debug",
                                        "USERPROFILE",
                                        "ConEmuBaseDirShort",
                                        "OLD_PATH",
                                        "IGCCSVC_DB",
                                        "currenArgu",
                                        "OS",
                                        "lib_base",
                                        "git_locale",
                                        "PROCESSOR_REVISION",
                                        "CMDER_INIT_END",
                                        "OneDriveConsumer",
                                        "ConEmuWorkDrive",
                                        "nix_tools",
                                        "USERDOMAIN_ROAMINGPROFILE",
                                        "debug_output",
                                        "ProgramData",
                                        "HOMEPATH",
                                        "feNot",
                                        "ConEmuDrawHWND",
                                        "add_path",
                                        "ConEmuHWND",
                                        "COLUMNS",
                                        "print_verbose",
                                        "PLINK_PROTOCOL",
                                        "SVN_SSH",
                                        "VENDORED_PATCH",
                                        "ConEmuWorkDir",
                                        "PROMPT",
                                        "VENDORED_MINOR",
                                        "ANSICON_DEF",
                                        "ConEmuServerPID",
                                        "verbose_output",
                                        "cexec",
                                        "ConEmuDrive",
                                        "ConEmuPID",
                                        "git_executable",
                                        "DriverData",
                                        "NUMBER_OF_PROCESSORS",
                                        "LINES",
                                        "PROCESSOR_IDENTIFIER",
                                        "time_init",
                                        "clink_dummy_capture_env",
                                        "DOTNET_ROOT_X64",
                                        "USERNAME",
                                        "ConEmuCfgDir",
                                        "ConEmuBackHWND",
                                        "ConEmuTask",
                                        "Path",
                                        "CommonProgramW6432",
                                        "PATHEXT",
                                        "CommonProgramFiles(x86)",
                                        "SystemRoot",
                                        "ConEmuHooks",
                                        "OneDrive",
                                        "user_aliases",
                                        "aliases",
                                        "VENDORED_MAJOR",
                                        "feFlagName",
                                        "lib_profile",
                                        "SESSIONNAME",
                                        "ProgramFiles",
                                        "ESC",
                                        "PROCESSOR_ARCHITECTURE",
                                        "CMDER_CLINK",
                                        "find_query",
                                        "CMDER_SHELL",
                                        "COMPUTERNAME",
                                        "ConEmuBaseDir",
                                        "EFC_75496",
                                        "max_depth",
                                        "APPDATA",
                                        "CMDER_ROOT",
                                        "print_error",
                                        "ccall",
                                        "TMP",
                                        "HOMEDRIVE",
                                        "add_to_path",
                                        "path_position",
                                        "ConEmuPalette",
                                        "CMDER_USER_FLAGS",
                                        "ProgramFiles(x86)",
                                        "ConEmuArgs",
                                        "ConEmuBuild",
                                        "fast_init",
                                        "depth",
                                        "CLINK_COMPLETIONS_DIR",
                                        "PROCESSOR_LEVEL",
                                        "ConEmuANSI",
                                        "windir",
                                        "PSModulePath",
                                        "lib_console",
                                        "ProgramW6432",
                                        "ALLUSERSPROFILE",
                                        "LOGONSERVER",
                                        "print_warning",
                                        "found",
                                        "SystemDrive",
                                        "CommonProgramFiles",
                                        "CMDER_CONFIGURED",
                                        "GIT_INSTALL_ROOT",
                                        "ANSICON",
                                        "architecture_bits",
                                        "ZES_ENABLE_SYSMAN",
                                        "PUBLIC",
                                        "USERDOMAIN",
                                        "HISTCONTROL",
                                        "XDG_SESSION_ID",
                                        "XDG_SESSION_TYPE",
                                        "SSH_CLIENT",
                                        "XDG_RUNTIME_DIR",
                                        "LS_COLORS",
                                        "BASH_FUNC_which%%",
                                        "SYSTEMD_COLORS",
                                        "MAIL",
                                        "LESSOPEN",
                                        "S_COLORS",
                                        "HISTSIZE",
                                        "which_declare",
                                        "DBUS_SESSION_BUS_ADDRESS",
                                        "XDG_SESSION_CLASS",
                                        "SSH_CONNECTION",
                                        "SSH_TTY",
                                        "MOTD_SHOWN",
                                        "TMUX",
                                        "TMUX_PANE",
                                        "_",
                                        "__CF_USER_TEXT_ENCODING",
                                        "__CFBundleIdentifier",
                                        "APPLICATION_INSIGHTS_NO_DIAGNOSTIC_CHANNEL",
                                        "COMMAND_MODE",
                                        "CommonPropertyBagPath",
                                        "CommonPropertyBagWithConfigPath",
                                        "ELECTRON_RUN_AS_NODE",
                                        "GPG_TTY",
                                        "HOME",
                                        "HOMEBREW_CELLAR",
                                        "HOMEBREW_PREFIX",
                                        "HOMEBREW_REPOSITORY",
                                        "INFOPATH",
                                        "LOGNAME",
                                        "MallocNanoZone",
                                        "MANPATH",
                                        "ORIGINAL_XDG_CURRENT_DESKTOP",
                                        "PATH",
                                        "PWD",
                                        "SHELL",
                                        "SHLVL",
                                        "SSH_AUTH_SOCK",
                                        "TMPDIR",
                                        "USER",
                                        "VSCODE_AMD_ENTRYPOINT",
                                        "VSCODE_CODE_CACHE_PATH",
                                        "VSCODE_CRASH_REPORTER_PROCESS_TYPE",
                                        "VSCODE_CWD",
                                        "VSCODE_HANDLES_UNCAUGHT_ERRORS",
                                        "VSCODE_IPC_HOOK",
                                        "VSCODE_L10N_BUNDLE_LOCATION",
                                        "VSCODE_NLS_CONFIG",
                                        "VSCODE_PID",
                                        "XPC_FLAGS",
                                        "XPC_SERVICE_NAME"};

    public static Dictionary<string, object> Get()
    {

        Dictionary<string, object> myDictionary = new()
        {
            { "date", DateTime.Now }
        };

        // Add environment variables from VariableInjectList
        foreach (var kvp in EnvironmentVariables.VariableInjectList)
        {
            var key = kvp.Key.ToString();
            if (!ignoreEnvWhenReporting.Contains(key))
            {
                myDictionary.Add(key, ParseValue(kvp.Value));
            }
        }

        // Add environment variables from system
        var entries = Environment.GetEnvironmentVariables();
        foreach (DictionaryEntry entry in entries)
        {
            var key = entry.Key?.ToString();
            var value = entry.Value?.ToString();
            
            if (key != null && value != null && 
                !ignoreList.Contains(key) && 
                !ignoreEnvWhenReporting.Contains(key) &&
                !myDictionary.ContainsKey(key))
            {
                myDictionary.Add(key, ParseValue(value));
            }
        }

        return myDictionary;
    }

    private static object ParseValue(string value)
    {
        // Try to parse as decimal
        if (decimal.TryParse(value, out decimal decimalValue))
        {
            return decimalValue;
        }

        // Try to parse as boolean
        if (bool.TryParse(value, out bool boolValue))
        {
            return boolValue;
        }

        // Return as string
        return value;
    }
}
