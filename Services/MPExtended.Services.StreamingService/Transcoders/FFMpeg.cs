#region Copyright (C) 2011-2013 MPExtended
// Copyright (C) 2011-2013 MPExtended Developers, http://www.mpextended.com/
// 
// MPExtended is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MPExtended is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MPExtended. If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Util;
using MPExtended.Services.StreamingService.Code;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.StreamingService.Units;

namespace MPExtended.Services.StreamingService.Transcoders
{
    internal class FFMpeg : ITranscoder
    {
        public string Identifier { get; set; }
        public StreamContext Context { get; set; }

        protected bool ReadOutputStream { get; set; }

        public FFMpeg()
        {
            ReadOutputStream = true;
        }

        public virtual string GetStreamURL()
        {
            return WCFUtil.GetCurrentRoot() + "StreamingService/stream/RetrieveStream?identifier=" + Identifier;
        }

        public virtual void BuildPipeline()
        {
            // add input
            bool doInputReader = Context.NeedsInputReaderUnit;
            if (doInputReader)
            {
                Context.Pipeline.AddDataUnit(Context.GetInputReaderUnit(), 1);
            }

            string arguments = GenerateArguments();

            // fix input thing
            if (!doInputReader)
                arguments = arguments.Replace("#IN#", Context.Source.GetPath());

            // add unit
            EncoderUnit.TransportMethod input = doInputReader ? EncoderUnit.TransportMethod.NamedPipe : EncoderUnit.TransportMethod.Other;
            EncoderUnit unit = new EncoderUnit(Configuration.StreamingProfiles.FFMpegPath, arguments, input, ReadOutputStream ? EncoderUnit.TransportMethod.NamedPipe : EncoderUnit.TransportMethod.Other, EncoderUnit.LogStream.StandardError, Context);
            unit.DebugOutput = false; // change this for debugging
            Context.Pipeline.AddDataUnit(unit, 5);

            // setup output parsing
            var einfo = new Reference<WebTranscodingInfo>(() => Context.TranscodingInfo, x => { Context.TranscodingInfo = x; });
            FFMpegLogParsingUnit logunit = new FFMpegLogParsingUnit(Context.Identifier, einfo, Context.StartPosition);
            logunit.LogMessages = true;
            logunit.LogProgress = true;
            Context.Pipeline.AddLogUnit(logunit, 6);
        }

        public virtual string GenerateArguments()
        {
            // calculate stream mappings (no way I'm going to add subtitle support; it's just broken)
            string videoMapping = Context.Profile.HasVideoStream ? "-map v:0 " : String.Empty;
            string audioMapping = Context.AudioTrackId != null ? String.Format("-map a:{0}", Context.MediaInfo.AudioStreams.First(x => x.ID == Context.AudioTrackId).Index) : String.Empty;

            // calculate size and aspect
            bool doResize = !Context.Profile.TranscoderParameters.ContainsKey("noResize") || Context.Profile.TranscoderParameters["noResize"] != "yes";
            string sizeAndAspect = Context.Profile.HasVideoStream && doResize ? String.Format("-s {0} -aspect {1}:{2}", Context.OutputSize, Context.OutputSize.Width, Context.OutputSize.Height) : String.Empty;

            // calculate start position and length
            string startPosition = Context.StartPosition != 0 ? String.Format("-ss {0}", Context.StartPosition / 1000) : String.Empty;
            string setLength = Context.Profile.TranscoderParameters.ContainsKey("setLength") ? String.Format("-t {0}", ((Context.MediaInfo.Duration - Context.StartPosition) / 1000.0).ToString("0.00", CultureInfo.InvariantCulture)) : String.Empty;

            // calculate output
            string output = ReadOutputStream ? " \"#OUT#\"" : String.Empty;

            // calculate full argument string
            string arguments = String.Format(
                    "-y {0} -i \"#IN#\" {1} {2} {3}{4} {5}{6}",
                    startPosition,
                    setLength,
                    sizeAndAspect,
                    videoMapping,
                    audioMapping,
                    Context.Profile.TranscoderParameters["codecParameters"],
                    output
                );

            return arguments;
        }
    }
}