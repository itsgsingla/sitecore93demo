module.exports = function () {
    var instanceRoot = "C:\\inetpub\\wwwroot\\demo930sc.dev.local";
    var config = {
        websiteRoot: instanceRoot + "\\",
        sitecoreLibraries: instanceRoot + "\\bin",
        licensePath: instanceRoot + "\\App_Data\\license.xml",
        solutionName: "Demo93",
        buildConfiguration: "Debug",
        buildToolsVersion: 15.0,
        buildMaxCpuCount: 0,
        buildVerbosity: "minimal",
        buildPlatform: "Any CPU",
        publishPlatform: "AnyCpu",
        runCleanBuilds: false
    };
    return config;
}
