using System.Diagnostics;
using System.IO;
using System.Management.Automation;


namespace CRtoJPG
{
    [Cmdlet(VerbsData.Convert, "CR2toJPG")]
    public class ConvertCr2ToJpg : Cmdlet
    {
        [Parameter(HelpMessage = "Input is mandatory", Mandatory = true, ValueFromPipeline = true)]
        public string Input { get; set; }

        [Parameter(HelpMessage = "Output path is mandatory", Mandatory = true, ValueFromPipeline = true)]
        public string Output { get; set; }


        private int Current { get; set; }
        private Stopwatch TotalTime { get; set; } = new Stopwatch();
        private Stopwatch ItemTime { get; set; } = new Stopwatch();

        //mean of time by each file
        private double Mean { get; set; }


        private ConverterOptions CObject { get; set; }

        protected override void BeginProcessing()
        {
            TotalTime.Start();

            //total files: CObject.Files.Length
            CObject = new ConverterOptions
            {
                Files = Directory.GetFiles(Input, "*.CR2"),
                OutputDirectory = Output
            };

            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {

            //check OutputFolder
            if (!Directory.Exists(CObject.OutputDirectory))
            {
                DirectoryInfo di = new DirectoryInfo(CObject.OutputDirectory);

                WriteObject($"Creating Directory {di.Name}");
                Directory.CreateDirectory(CObject.OutputDirectory);
            }

            foreach (var file in CObject.Files)
            {
                ItemTime.Start();
                Current++;
                FileInfo fi = new FileInfo(file);
                WriteObject($"Working with file \"{fi.Name}\" {Current}/{CObject.Files.Length} {Current * 100 / CObject.Files.Length}%");
                Converter.ConvertImage(file, CObject.OutputDirectory);

                ItemTime.Stop();
                Mean += ItemTime.Elapsed.TotalSeconds;
                ItemTime.Reset();
            }
            base.ProcessRecord();
        }

        protected override void EndProcessing()
        {
            TotalTime.Stop();

            WriteObject($"Finished, total time: {TotalTime.Elapsed.Seconds} seg - Speed {Mean / (double)CObject.Files.Length} sec/picture");
            base.EndProcessing();
        }

        protected override void StopProcessing()
        {
            TotalTime.Stop();

            WriteObject($"Process Interrupted - total time: {TotalTime.Elapsed.Seconds} seg");
            base.StopProcessing();
        }




    }
}
