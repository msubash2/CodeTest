// See https://aka.ms/new-console-template for more information

using DocoptNet;

const string usage = 
    """
    A patch file diffing tool (LightKeeper backend developer code test)

    Usage:
      patch_diff (-h | --help)
      patch_diff <patch_left> <patch_right>

    Options:
      -h --help                  Show this screen.
      <patch_left>               Path to a patch file.
      <patch_right>              Path to another patch file.

    """;

var arguments = new Docopt().Apply(usage, args, version: "patch_diff 0.1", exit: true);
if (arguments == null) {
    // faulty docopt configuration?
    throw new Exception("no argument list");
}

// Pre-flight checks
var pathLeft = arguments["<patch_left>"].ToString();
var pathRight = arguments["<patch_right>"].ToString();
if (string.IsNullOrWhiteSpace(pathLeft)) {
    // faulty docopt configuration?
    throw new Exception($"Missing required parameter: {nameof(pathLeft)}");
}
if (string.IsNullOrWhiteSpace(pathRight)) {
    // faulty docopt configuration?
    throw new Exception($"Missing required parameter: {nameof(pathRight)}");
}
var absPathLeft = Path.GetFullPath(pathLeft);
var absPathRight = Path.GetFullPath(pathRight);

// Compare Path Files
Patch.Patch.ComparePatchFiles(absPathLeft, absPathRight);

