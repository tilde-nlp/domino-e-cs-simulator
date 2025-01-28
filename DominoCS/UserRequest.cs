using Microsoft.AspNetCore.Mvc;
using Microsoft.JSInterop.Implementation;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace DominoCS
{
    public class UserRequest
    {
        public string? externalId { get; set; }
        public string? status { get; set; }
        public Aoi aoi { get; set; }
        public Validity validity { get; set; }
        public int priority { get; set; }
        public List<MissionParameter>? userRequestMissionParameters { get; set; }
        public List<ProgrammingRequest>? programmingRequests { get; set; }

        public static bool goodString(string parm)
        {
            var reg = new Regex(@"^[A-Za-z0-9 !\""#$%&'*+,-./:;<=>?@^_`(|)~]{1,500}$");
            return reg.IsMatch(parm);
        }


        public string validate()
        {         
            var res = "";

            if (!string.IsNullOrEmpty(externalId))
                if (!goodString(externalId))
                    res += $"externalId invalid\n";

            var validStatuses = new List<string> { "CREATED", "ANALYZED", "ACTIVATED", "COMPLETED", "CANCELLED" };
            if (!string.IsNullOrEmpty(status) && !validStatuses.Contains(status))
                res += $"invalid status\n";

            if (aoi == null)
                res += $"aoi is null\n";
            else
                res += aoi.validate("Polygon", ": aoi");

            if (validity == null)
                res += $"validity is null\n";
            else
                res += validity.validate("validity");

            if (userRequestMissionParameters == null)
                res += $"userRequestMissionParameters is null\n";
            else
                for(int i = 0; i < userRequestMissionParameters.Count; i++)
                {
                    res += userRequestMissionParameters[i].validate($"userRequestMissionParameter {i}");
                }

            if (programmingRequests != null && programmingRequests.Any())                            
                for (int i = 0; i < programmingRequests.Count; i++)
                {
                    res += programmingRequests[i].validate($"programmingRequest {i}");
                }

            return res;
        }
    }

    public class Aoi
    {
        public string type { get; set; }
        public List<List<List<float>>> coordinates { get; set; }
        public List<float> bbox { get; set; }

        public string validate(string allowedType, string prefix)
        {
            var res = "";

            if (type != allowedType)
                res += $"{prefix}: Invalid type\n";

            if (coordinates == null || !coordinates.Any() || !coordinates.First().Any() || !coordinates.First().First().Any())
                res += $"{prefix}: Empty coordinates\n";

            if (bbox != null && bbox.Count < 4)
                res += $"{prefix}: bbox not enough numbers\n";

            return res;
        }
    }

    public class Aoi4
    {
        public string type { get; set; }
        public List<List<List<List<float>>>> coordinates { get; set; }
        public List<float> bbox { get; set; }

        public string validate(string allowedType, string prefix)
        {
            var res = "";

            if (type != allowedType)
                res += $"{prefix}: Invalid type\n";

            if (coordinates == null || !coordinates.Any() || !coordinates.First().Any() || !coordinates.First().First().Any() || !coordinates.First().First().First().Any())
                res += $"{prefix}: Empty coordinates\n";

            if (bbox != null && bbox.Count < 4)
                res += $"{prefix}: bbox not enough numbers\n";

            return res;
        }
    }

    public class Validity
    {
        public string begin { get; set; }
        public string end { get; set; }

        public string validate(string prefix)
        {
            var res = "";

            DateTime beginDate;
            if (!DateTime.TryParseExact(begin, "yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture, DateTimeStyles.None, out beginDate))
                res += $"{prefix}: failed to parse begin\n";
            DateTime endDate;
            if (!DateTime.TryParseExact(end, "yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture, DateTimeStyles.None, out beginDate))
                res += $"{prefix}: failed to parse end\n";

            return res;
        }
    }

    public class MissionParameter
    {

        public string? constellation { get; set; }
        public List<string>? satellites { get; set; }
        public List<string>? downloadStations { get; set; }
        public string? cloudCoverNotationMode { get; set; }
        public float clearSkyRejectionSelectionThreshold { get; set; }
        public float clearSkyRejectionValidationThreshold { get; set; }
        public int priority { get; set; }
        public float cap { get; set; }
        public bool monopass { get; set; }
        public string? acquisitionMode { get; set; }
        public string? angularConstraintsType { get; set; }
        public float bhMin { get; set; }
        public float bhMax { get; set; }
        public float psiXMin { get; set; }
        public float psiXMax { get; set; }
        public float psiYMin { get; set; }
        public float psiYMax { get; set; }
        public float psyXYMax { get; set; }

        public string validate(string prefix)
        {
            var res = "";

            if (!string.IsNullOrEmpty(cloudCoverNotationMode) && cloudCoverNotationMode != "MANUAL" && cloudCoverNotationMode != "AUTOMATIC")
                res += $"{prefix}: cloudCoverNotationMode invalid\n";

            if (clearSkyRejectionSelectionThreshold < 0 || clearSkyRejectionSelectionThreshold > 100)
                res += $"{prefix}: clearSkyRejectionSelectionThreshold invalid\n";

            if (clearSkyRejectionValidationThreshold < 0 || clearSkyRejectionValidationThreshold > 100)
                res += $"{prefix}: clearSkyRejectionValidationThreshold invalid\n";

            if (priority < 0 || priority > 16)
                res += $"{prefix}: priority invalid\n";

            var validAcquisitionModes = new List<string> { "MONOSCOPIC", "STEREO", "TRISTEREO", "N_UPLET" };
            if (!string.IsNullOrEmpty(acquisitionMode) && !validAcquisitionModes.Contains(acquisitionMode))
                res += $"{prefix}: acquisitionMode invalid\n";

            if (!string.IsNullOrEmpty(angularConstraintsType) && angularConstraintsType != "DEPOINTING")
                res += $"{prefix}: angularConstraintsType invalid\n";

            return res;
        }
    }

    public class ScoringParameters
    {
        public bool areaWeightingFlag { get; set; }
        public bool coreWeightingFlag { get; set; }
        public bool catalogWeightingFlag { get; set; }
        public bool weatherWeightingFlag { get; set; }
        public int clearSkyRejectionThreshold   { get; set; }
        public int clearSkyBonusThreshold { get; set; }
        public JObject? missionParameters { get; set; }

        public string validate(string prefix)
        {
            var res = "";

            if (clearSkyBonusThreshold < 0 || clearSkyBonusThreshold > 100)
                res += $"{prefix}: clearSkyBonusThreshold invalid\n";

            if (clearSkyRejectionThreshold < 0 || clearSkyRejectionThreshold > 100)
                res += $"{prefix}: clearSkyRejectionThreshold invalid\n";

            return res;
        }
    }

    public class SplitParameters
    {
        public string? method { get; set; }
        public float maxMeshLength { get; set; }
        public float cap {  get; set; }
        public float overlapMarginAlongTrack { get; set; }
        public float overlapMarginAcrossTrack { get; set; }
        public JObject? missionParameters { get; set; }

        public string validate(string prefix)
        {
            var res = "";

            var validMethods = new List<string> { "WORLD_LAYERED_SPLIT", "DYNAMIC_FIXED_ORIENTATION", "IMPOSED_MESH" };
            if (!string.IsNullOrEmpty(method) && !validMethods.Contains(method))
                res += $"{prefix}: invalid method\n";
            
            return res;
        }
    }

    public class AcquisitionParameters
    {
        public string? acquisitionType { get; set; }
        public string? mode { get; set; }
        public bool monopass { get; set; }
        public string? stereoType { get; set; }
        public int luminosityThreshold { get; set; }
        public string? orbitalPhase { get; set; }
        public string? guidanceMode { get; set; }
        public int nUpletAcquisitionsNumber { get; set; }
        public JObject? missionParameters { get; set; }

        public string validate(string prefix)
        {
            var res = "";

            var validModes = new List<string> { "MONOSCOPIC", "STEREO", "TRISTEREO", "QUADRISTEREO", "N_UPLET" };
            if (!string.IsNullOrEmpty(mode) && !validModes.Contains(mode))
                res += $"{prefix}: invalid mode\n";

            var validStereotypes = new List<string> { "QUASI_SYMETRICAL", "FORWARD_BACKWARD" };
            if (!string.IsNullOrEmpty(stereoType) && !validStereotypes.Contains(stereoType))
                res += $"{prefix}: invalid stereoType\n";

            if (luminosityThreshold < 0 || luminosityThreshold > 90)
                res += $"{prefix}: luminosityThreshold invalid\n";

            var validPhases = new List<string> { "DAY", "NIGHT" };
            if (!string.IsNullOrEmpty(orbitalPhase) && !validPhases.Contains(orbitalPhase))
                res += $"{prefix}: invalid orbitalPhase\n";

            if (nUpletAcquisitionsNumber > 0 && mode != "N_UPLET")
            {
                res += $"{prefix}: invalid nUpletAcquisitionsNumber\n";
            }

            return res;
        }
    }

    public class DownloadParameters
    {
        public class DownloadBranch
        {
            public string? inventoryCenter { get; set; }
            public List<string>? stations { get; set; }
            public bool validating { get; set; }
            public JObject? missionParameters { get; set; }

            public string validate(string prefix)
            {
                var res = "";

                if (string.IsNullOrEmpty(inventoryCenter) && !new Regex("^[A-Za-z0-9]{3,4}$").IsMatch(inventoryCenter))
                    res += $"{prefix}: inventoryCenter invalid\n";

                return res;
            }
        }

        public List<DownloadBranch>? downloadBranches { get; set; }
        public JObject? missionParameters { get; set; }
        public string validate(string prefix)
        {
            var res = "";

            if (downloadBranches != null)
            {
                for (int i = 0; i < downloadBranches.Count; i++)
                {
                    res += downloadBranches[i].validate($"{prefix}: downloadBranches {i}");
                }
            }

            return res;
        }
    }

    public class CoverageCompletion
    {
        public class Progress
        {
            public string date { get; set; }
            public float progress { get; set; }

            public string validate(string prefix)
            {
                var res = "";

                if (!DateTime.TryParseExact(date, "yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                    res += $"{prefix}: failed to parse date\n";

                if (progress < 0 || progress > 100)
                    res += $"{prefix}: invalid progress\n";

                return res;
            }
        }

        public int simulationYear { get; set; }
        public bool? completed { get; set; }
        public string? completionDate { get; set; }
        public float progressAtDate { get; set; }
        public List<Progress> progress { get; set; }

        public string validate(string prefix)
        {
            var res = "";

            if (completed == null)
                res += $"{prefix}: invalid completed\n";

            if (completed == true)
            {
                if (string.IsNullOrEmpty(completionDate))
                    res += $"{prefix}: invalid completion date\n";
                else
                if (!DateTime.TryParseExact(completionDate, "yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                    res += $"{prefix}: failed to parse completionDate\n";
            }

            if (progressAtDate < 0 || progressAtDate > 100)
                res += $"{prefix}: invalid progressAtDate\n";

            if(progress == null)
                res += $"{prefix}: missing progress\n";
            else
                for(int i = 0; i<progress.Count;i++)
                {
                    res += progress[i].validate($"{prefix}: progress {i}");
                }

            return res;
        }

    }

    public class PsoHolder
    {
        public Pso? pso { get; set; }
        public string validate(string prefix)
        {
            var res = "";

            if (pso != null)
                res += pso.validate($"{prefix}: pso");

            return res;
        }
    }

    public class Pso
    {
        public int orbitNumber { get; set; }
        public float durationFromStart { get; set; }

        public string validate(string prefix)
        {
            var res = "";

            if (orbitNumber < 0)
                res += $"{prefix}: orbitNumber invalid";

            if (durationFromStart < 0)
                res += $"{prefix}: durationFromStart invalid";

            return res;
        }
    }

    public class DatedDto
    {
        public string? guid { get; set; }
        public string? satellite { get; set; }
        public string? orbitalDirection { get; set; }
        public float accessRoll { get; set; }
        public int orbitNumber { get; set; }
        public PsoHolder? psoStart { get; set; }
        public PsoHolder? psoMiddle { get; set; }
        public PsoHolder? psoEnd { get; set; }
        public JObject? missionParameters { get; set;}
        public string? externalId { get; set; }
        public string? priority { get; set; }
        public Validity? period { get; set; }
        public int orbitCycleNumber { get; set; }
        public float solarElevationAngle { get; set; }

        public string validate(string prefix)
        {
            var res = "";

            if (orbitalDirection != null && orbitalDirection != "ASCENDING" && orbitalDirection != "DESCENDING")
                res += $"{prefix}: orbitalDirection invalid";

            if (orbitNumber < 0)
                res += $"{prefix}: orbitNumber invalid";

            if (orbitCycleNumber < 0)
                res += $"{prefix}: orbitCycleNumber invalid";

            if (psoStart != null)
                res += psoStart.validate($"{prefix}: psoStart");

            if (psoMiddle != null)
                res += psoMiddle.validate($"{prefix}: psoMiddle");

            if (psoEnd != null)
                res += psoEnd.validate($"{prefix}: psoEnd");

            var validPrios = new List<string> { "NONE", "LOW", "NORMAL", "HIGH", "FULL" };
            if (!string.IsNullOrEmpty(priority) && !validPrios.Contains(priority))
                res += $"{prefix}: invalid priority\n";

            if (period != null)
                res += period.validate($"{prefix}: period");

            if (solarElevationAngle < 0 || solarElevationAngle > 90)
                res += $"{prefix}: solarElevationAngle invalid";

            return res;
        }
    }

    public class Mesh
    {
        public string? guid { get; set; }
        public Aoi? aoi { get; set; }
        public float meanAcquisitionDuration { get; set; }
        public float usefulArea { get; set; }
        public List<string>? gridCellIds { get; set; }
        public JObject? missionParameters { get; set; }

        public string validate(string prefix)
        {
            var res = "";

            if (aoi != null)
                res += aoi.validate("Polygon",$"{prefix}: aoi");

            if (meanAcquisitionDuration < 0)
                res += $"{prefix}: meanAcquisitionDuration invalid";

            if (usefulArea < 0)
                res += $"{prefix}: usefulArea invalid";

            return res;
        }
    }

    public class Acquisition
    {
        public string? externalId { get; set; }
        public string? status { get; set; }
        public List<DatedDto>? datedDtos { get; set; }
        public List<Mesh>? meshes { get; set; }
        public JObject? missionParameters { get; set; }

        public string validate(string prefix)
        {
            var res = "";

            if (externalId != null && !new Regex("^ACQR_[0-9]+_[0-9]+_[0-9]+$").IsMatch(externalId))
                res += $"{prefix}: invalid externalid\n";

            var validStates = new List<string> { "ACTIVATED", "CANCELLED", "COMPLETED" };
            if (!string.IsNullOrEmpty(status) && !validStates.Contains(status))
                res += $"{prefix}: invalid status\n";

            if (datedDtos != null)
            {
                for (int i = 0; i < datedDtos.Count; i++)
                {
                    res += datedDtos[i].validate($"{prefix}: dateDtos {i}");
                }
            }

            if (meshes != null)
            {
                for (int i = 0; i < meshes.Count; i++)
                {
                    res += meshes[i].validate($"{prefix}: meshes {i}");
                }
            }

            return res;
        }
    }

    public class ProgrammingRequest
    {
        public string? externalId { get; set; }
        public string constellation { get; set; }

        public int priority { get; set; }
        public string status { get; set; }
        public Validity validity { get; set; }

        public string? activationDate { get; set; }
        public Aoi4 aoi { get; set; }
        public Aoi4 remainingToAcquire { get; set; }
        public List<string> satellites { get; set; }

        public ScoringParameters? scoringParameters { get; set; }
        public SplitParameters? splitParameters { get; set; }
        public AcquisitionParameters? acquisitionParameters { get; set; }

        public JObject? angularConstraints { get; set; }
        public DownloadParameters? downloadParameters { get; set; }
        public List<CoverageCompletion>? initialCoverageCompletion { get; set; }
        public List<CoverageCompletion>? latestCoverageCompletion { get; set; }
        public JObject? missionParameters { get; set; }
        public List<Acquisition>? acquisitions { get; set; }

        public string validate(string prefix)
        {
            var res = "";

            if (!string.IsNullOrEmpty(externalId) && !new Regex("^PROGR_[0-9]+_[0-9]+$").IsMatch(externalId))


            if (string.IsNullOrEmpty(constellation) || !UserRequest.goodString(constellation))
                res += $"{prefix}: constellation invalid\n";

            if (priority < 0 || priority > 16)
                res += $"{prefix}: priority invalid\n";

            var validStatuses = new List<string> { "ACTIVATED", "CANCELLED", "EXPIRED", "COMPLETED" };
            if (!string.IsNullOrEmpty(status) && !validStatuses.Contains(status))
                res += $"{prefix}: status invalid\n";
            
            if (validity != null)
                res += validity.validate($"{prefix}: validity");

            if (aoi == null)
                res += $"{prefix}: aoi is null\n";
            else
            {
                res += aoi.validate("MultiPolygon", prefix + ": aoi");                
            }

            if (remainingToAcquire != null)
            {
                var aval = remainingToAcquire.validate("MultiPolygon", prefix + ": remainingToAcquire");
                if (!string.IsNullOrEmpty(aval))
                    res += $"{prefix}: remainingToAcquire {aval}";
            }

            if (satellites == null || !satellites.Any())
                res += $"{prefix}: satellites invalid\n";

            if (scoringParameters != null)
            {
                res += scoringParameters.validate(prefix + ": scoringParameters");                
            }

            if (splitParameters != null)
            {
                res += splitParameters.validate(prefix + ": splitParameters");                
            }
            if (acquisitionParameters != null)
            {
                res += acquisitionParameters.validate(prefix + ": acquisitionParameters");                
            }

            //angularConstraints can be 3 different objects, but nothing aside from type requires validation
            if(angularConstraints!= null)
            {
                string angType = angularConstraints["angularConstraintsType"]?.ToString() ?? "";
                var validTypes = new List<string> { "INCIDENCE", "DEPOINTING", "FORWARD_BACKWARD" };
                if (!validTypes.Contains(angType))
                    res += $"{prefix}: angularConstraints type invalid\n";
            }

            if (downloadParameters != null)
            {
                res += downloadParameters.validate(prefix + ": downloadParameters");
            }

            if (initialCoverageCompletion != null)
            {
                for (int i = 0; i < initialCoverageCompletion.Count; i++)
                {
                    res += initialCoverageCompletion[i].validate($"{prefix}: initialCoverageCompletion {i}");
                }
            }

            if (latestCoverageCompletion != null)
            {
                for (int i = 0; i < latestCoverageCompletion.Count; i++)
                {
                    res += latestCoverageCompletion[i].validate($"{prefix}: latestCoverageCompletion {i}");
                }
            }

            if (acquisitions != null)
            {
                for (int i = 0; i < acquisitions.Count; i++)
                {
                    res += acquisitions[i].validate($"{prefix}: acquisition {i}");
                }
            }

            return res;
        }
    }
}
