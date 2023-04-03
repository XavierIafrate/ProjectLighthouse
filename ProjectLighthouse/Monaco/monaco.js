let snippets = [
	{
		label: 'tool comment',
		kind: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
		documentation: 'Test.',
		insertText: '(T${1:0} ${2:NOT USED})',
		insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
	},
	{
		label: 'tools',
		kind: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
		documentation: 'Test.',
		insertText: '(TOOLS)\n(T1 ${1:NOT USED})\n(T2 ${2:NOT USED})\n(T4 ${3:NOT USED})\n(T3 ${4:NOT USED})\n(T5 ${5:NOT USED})\n(T35 ${6:NOT USED})\n(T36 ${7:NOT USED})\n(T37 ${8:NOT USED})\n(T34 ${9:NOT USED})\n(T31 ${10:NOT USED})',
		insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,

	},
	{
		label: 'GXYZ',
		kind: monaco.languages.CompletionItemKind.Function,
		documentation: 'GXYZ',
		insertText: 'G${1:_}X${2:_}Y${3:_}Z${4:_}',
		insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,

	},
	{
		label: 'IF-GOTO',
		kind: monaco.languages.CompletionItemKind.Function,
		documentation: 'test',
		insertText: 'IF[#${1:VAR} ${2:EQ} ${3:0.0}] GOTO ${4:LINE}',
		insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,

	},
	{
		label: 'IF-THEN',
		kind: monaco.languages.CompletionItemKind.Function,
		documentation: 'test',
		insertText: 'IF[#${1:VAR} ${2:EQ} ${3:0.0}] THEN ${4:statement}',
		insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,

	},
];

let hints = [];

let codeDefinitions = {
	"G0": { description: "Rapid Motion Positioning", group: "1" },
	"G1": { description: "Linear Interpolation Motion", group: "1" },
	"G2": { description: "CW Circular Interpolation Motion", group: "1" },
	"G3": { description: "CCW Circular Interpolation Motion", group: "1" },
	"G4": { description: "Dwell", group: "0" },
	"G9": { description: "Exact Stop", group: "0" },
	"G10": { description: "Set Offsets", group: "0" },
	"G12": { description: "Circular Pocket Milling CW", group: "" },
	"G13": { description: "Circular Pocket Milling CCW", group: "" },
	"G14": { description: "Secondary Spindle Swap", group: "17" },
	"G15": { description: "Secondary Spindle Swap Cancel", group: "17" },
	"G17": { description: "XY Plane", group: "2" },
	"G18": { description: "XZ Plane", group: "2" },
	"G19": { description: "YZ Plane", group: "2" },
	"G20": { description: "Select Inches", group: "6" },
	"G21": { description: "Select Metric", group: "6" },
	"G28": { description: "Return To Machine Zero Point", group: "0" },
	"G29": { description: "Return From Reference Point", group: "0" },
	"G31": { description: "Skip Function", group: "0" },
	"G32": { description: "Thread Cutting", group: "1" },
	"G40": { description: "Tool Nose Compensation Cancel", group: "7" },
	"G41": { description: "Tool Nose Compensation (TNC) Left", group: "7" },
	"G42": { description: "Tool Nose Compensation (TNC) Right", group: "7" },
	"G43": { description: "Tool Length Compensation + (Add)", group: "8" },
	"G44": { description: "Tool Length Compensation - (Subtract)", group: "8" },
	"G50": { description: "Spindle Speed Limit", group: "0" },
	"G50": { description: "Set Global coordinate Offset FANUC", group: "0" },
	"G52": { description: "Set Local Coordinate System FANUC", group: "0" },
	"G53": { description: "Machine Coordinate Selection", group: "0" },
	"G54": { description: "Coordinate System #1 FANUC", group: "12" },
	"G55": { description: "Coordinate System #2 FANUC", group: "12" },
	"G56": { description: "Coordinate System #3 FANUC", group: "12" },
	"G57": { description: "Coordinate System #4 FANUC", group: "12" },
	"G58": { description: "Coordinate System #5 FANUC", group: "12" },
	"G59": { description: "Coordinate System #6 FANUC", group: "12" },
	"G61": { description: "Exact Stop Modal", group: "15" },
	"G64": { description: "Exact Stop Cancel G61", group: "15" },
	"G65": { description: "Macro Subprogram Call Option", group: "0" },
	"G68": { description: "Rotation", group: "16" },
	"G69": { description: "Cancel G68 Rotation", group: "16" },
	"G70": { description: "Finishing Cycle", group: "0" },
	"G71": { description: "O.D./I.D. Stock Removal Cycle", group: "0" },
	"G72": { description: "End Face Stock Removal Cycle", group: "0" },
	"G73": { description: "Irregular Path Stock Removal Cycle", group: "0" },
	"G74": { description: "End Face Grooving Cycle", group: "0" },
	"G75": { description: "O.D./I.D. Grooving Cycle", group: "0" },
	"G76": { description: "Threading Cycle, Multiple Pass", group: "0" },
	"G80": { description: "Canned Cycle Cancel", group: "9" },
	"G81": { description: "Drill Canned Cycle", group: "9" },
	"G82": { description: "Spot Drill Canned Cycle", group: "9" },
	"G83": { description: "Normal Peck Drilling Canned Cycle", group: "9" },
	"G84": { description: "Tapping Canned Cycle", group: "9" },
	"G85": { description: "Boring Canned Cycle", group: "9" },
	"G86": { description: "Bore and Stop Canned Cycle", group: "9" },
	"G89": { description: "Bore and Dwell Canned Cycle", group: "9" },
	"G90": { description: "O.D./I.D. Turning Cycle", group: "1" },
	"G92": { description: "Threading Cycle", group: "1" },
	"G94": { description: "End Facing Cycle", group: "1" },
	"G95": { description: "Live Tooling Rigid Tap (Face)", group: "9" },
	"G96": { description: "Constant Surface Speed On", group: "13" },
	"G97": { description: "Constant Surface Speed Off", group: "13" },
	"G98": { description: "Feed Per Minute", group: "10" },
	"G99": { description: "Feed Per Revolution", group: "10" },
	"G100": { description: "Disable Mirror Image", group: "0" },
	"G101": { description: "Enable Mirror Image", group: "0" },
	"G103": { description: "Limit Block Lookahead", group: "0" },
	"G105": { description: "Servo Bar Command", group: "9" },
	"G107": { description: "G107 Cylindrical Mapping", group: "0" },
	"G110": { description: "Coordinate System #7", group: "12" },
	"G111": { description: "Coordinate System #8", group: "12" },
	"G112": { description: "XY to XC Interpolation", group: "4" },
	"G113": { description: "Cancel G112", group: "4" },
	"G114": { description: "Coordinate System #9", group: "12" },
	"G115": { description: "Coordinate System #10", group: "12" },
	"G116": { description: "Coordinate System #11", group: "12" },
	"G117": { description: "Coordinate System #12", group: "12" },
	"G118": { description: "Coordinate System #13", group: "12" },
	"G119": { description: "Coordinate System #14", group: "12" },
	"G120": { description: "Coordinate System #15", group: "12" },
	"G121": { description: "Coordinate System #16", group: "12" },
	"G122": { description: "Coordinate System #17", group: "12" },
	"G123": { description: "Coordinate System #18", group: "12" },
	"G124": { description: "Coordinate System #19", group: "12" },
	"G125": { description: "Coordinate System #20", group: "12" },
	"G126": { description: "Coordinate System #21", group: "12" },
	"G127": { description: "Coordinate System #22", group: "12" },
	"G128": { description: "Coordinate System #23", group: "12" },
	"G129": { description: "Coordinate System #24", group: "12" },
	"G154": { description: "Select Work Coordinates P1-99", group: "12" },
	"G156": { description: "Broaching Canned Cycle", group: "9" },
	"G165": { description: "LFV Commands", group: "" },
	"G167": { description: "Modify Setting", group: "0" },
	"G170": { description: "G170 Cancel G171/G172", group: "20" },
	"G171": { description: "G171 Radius Programming Override", group: "20" },
	"G172": { description: "G172 Diameter Programming Override", group: "20" },
	"G184": { description: "Reverse Tapping Canned Cycle For Left Hand Threads", group: "9" },
	"G186": { description: "Reverse Live Tool Rigid Tap (For Left Hand Threads)", group: "9" },
	"G187": { description: "Accuracy Control", group: "0" },
	"G195": { description: "Forward Live Tool Radial Tapping (Diameter)", group: "9" },
	"G196": { description: "Reverse Live Tool Radial Tapping (Diameter)", group: "9" },
	"G198": { description: "Disengage Synchronous Spindle Control", group: "0" },
	"G199": { description: "Engage Synchronous Spindle Control", group: "0" },
	"G200": { description: "Index on the Fly", group: "0" },
	"G211": { description: "Manual Tool Setting", group: "-" },
	"G212": { description: "Auto Tool Setting", group: "-" },
	"G234": { description: "Tool Center Point Control (TCPC)", group: "8" },
	"G241": { description: "Radial Drill Canned Cycle", group: "9" },
	"G242": { description: "Radial Spot Drill Canned Cycle", group: "9" },
	"G243": { description: "Radial Normal Peck Drilling Canned Cycle", group: "9" },
	"G245": { description: "Radial Boring Canned Cycle", group: "9" },
	"G246": { description: "Radial Bore and Stop Canned Cycle", group: "9" },
	"G249": { description: "Radial Bore and Dwell Canned Cycle", group: "9" },
	"G250": { description: "Cancel Scaling", group: "11" },
	"G251": { description: "Scaling", group: "11" },
	"G254": { description: "Dynamic Work Offset (DWO)", group: "23" },
	"G255": { description: "Cancel Dynamic Work Offset (DWO)", group: "23" },
	"G266": { description: "Visible Axes Linear Rapid %Motion", group: "0" },
	"G268": { description: "Enable Feature Coordinate System", group: "2" },
	"G269": { description: "Disable Feature Coordinate System", group: "2" },
	"G390": { description: "Absolute Position Command", group: "3" },
	"G391": { description: "Incremental Position Command", group: "3" },
	"G600": { description: "Free Machining Mode", group: "" },
	"G630": { description: "Front back parallel machining", group: "" },
	"G650": { description: "Pick off / centre support", group: "" },
	"G999": { description: "Last part mode enabled", group: "" },
	"M0": "Program Stop",
	"M1": "Optional Stop",
	"M2": "Single cycle stop",
	"M3": "SP1 forward",
	"M4": "SP1 reverse",
	"M5": "SP1 stop",
	"M6": "SP1 collet close",
	"M7": "SP1 collet open",
	"M8": "Check for end of bar",
	"M9": "Reset end of bar",
	"M10": "Ejector pin forward",
	"M11": "Ejector pin retract",
	"M12": "RGB Torsion correct",
	"M13": "SP 1 C axis ON",
	"M14": "Long workpiece chuck open (Type 1)",
	"M15": "SP2 collet close",
	"M16": "SP2 collet open",
	"M17": "SP1 chuck open",
	"M18": "SP1 C axis",
	"M20": "SP1 C axis/Indexing cancel",
	"M21": "Notch ON",
	"M22": "Notch OFF",
	"M23": "SP2 forward",
	"M24": "SP2 reverse",
	"M25": "SP2 stop",
	"M26": "Barchange phase checking",
	"M27": "Tail stock retract",
	"M28": "One degree indexing",
	"M29": "SP1 rigid tapping",
	"M30": "Program end",
	"M31": "Workpiece conveyor ON",
	"M32": "Part collection macro",
	"M33": "Part unload part macro",
	"M34": "Part unload macro",
	"M35": "Parts catcher advance",
	"M36": "Guide bush close",
	"M37": "Guide bush open",
	"M38": "Part catcher advanced cycle movement (option)",
	"M39": "Bar loader torque ON / Synchro control OFF",
	"M40": "Z2 axis forward",
	"M41": "Z2 axis retract",
	"M42": "Vacuum ON",
	"M43": "Vacuum OFF",
	"M44": "Move collection basket to 75 degrees",
	"M45": "Move collection basket to 285 degrees",
	"M46": "Bar Loader zero torque",
	"M47": "Automatic measuring sensor up",
	"M48": "SP2 C axis",
	"M49": "Enable 100% override clamp",
	"M50": "Partoff detect",
	"M51": "Partoff probe",
	"M52": "Coolant ON",
	"M53": "Coolant OFF",
	"M54": "Barfeed pushing OFF",
	"M55": "Barfeed pushing ON/load new bar",
	"M56": "Product count",
	"M57": "One cycle stop",
	"M58": "SP3 Power tools forward",
	"M59": "SP3 Power tools reverse",
	"M60": "SP3 Power tools stop",
	"M61": "Time contact of rapid feed is ON for X1 and Z1 thread cutting",
	"M62": "External M code",
	"M63": "External M code",
	"M64": "External M code",
	"M65": "External M code",
	"M66": "External M code",
	"M67": "Power tool left hand rigid tap ON",
	"M68": "External M code",
	"M69": "External M code",
	"M70": "Zero point return of C axis",
	"M71": "Spare M code ",
	"M72": "SP2 airblast ON",
	"M73": "SP2 airblast OFF",
	"M74": "Spindle synchronization enable",
	"M75": "Face power tools reverse",
	"M76": "Disable interference zone around top tool block",
	"M77": "Check spindle synchronization",
	"M78": "SP2 indexing",
	"M79": "SP2 indexing cancel",
	"M80": "SP3 Power tools forward",
	"M81": "Spindle synchro control offset effective",
	"M82": "SP3 Power tools stop",
	"M83": "T2** power tools forward",
	"M84": "Turret power tools reverse",
	"M85": "Turret power tools stop",
	"M86": "SP2 fluctuation detection ON",
	"M87": "Cutting start interlock OFF",
	"M88": "Interference check OFF",
	"M89": "Interference check ON",
	"M90": "Error detect ON",
	"M91": "SP1 encoder ON",
	"M92": "Error detect ON",
	"M93": "Error detect OFF",
	"M94": "SP2 fluctuation detection ON",
	"M95": "Work measure start",
	"M96": "SP1 fluctuation detection ON",
	"M97": "SP1 fluctuation detection OFF",
	"M98": "Call subroutine / subprogram",
	"M99": "End of program or end of subprogram/subroutine",
	"M100": "Grip bar stock",
	"M101": "Release bar stock",
	"M102": "Release bar stock quickly",
	"M103": "Release bar stock,Slide channel forward",
	"M104": "Slide channel forward",
	"M105": "Slide channel back",
	"M106": "Stabiliser close",
	"M107": "Stabiliser open",
	"M108": "Master Measure start",
	"M109": "Barchange subprogram call",
	"M110": "U35J air blow ON",
	"M111": "U35J air blow OFF",
	"M112": "Input guide bush phase data",
	"M113": "Sequential operation for long workpiece",
	"M114": "Automatic measure sensor centre",
	"M115": "Centering is completed",
	"M116": "U35J Hand Close",
	"M117": "SP2 phase memory",
	"M118": "Measuring jaw close",
	"M119": "Measuring jaw open",
	"M120": "SP2 phase memory",
	"M121": "Measuring jaw close",
	"M122": "Measuring jaw open",
	"M123": "Back torque limit ON",
	"M124": "Back torque limit OFF",
	"M125": "NC Reset and rewind",
	"M126": "Non conditional optional stop",
	"M127": "Request for recalculation",
	"M128": "Optional subspindle power tool spindle forward",
	"M129": "Optional subspindle power tool spindle reverse",
	"M130": "Optional subspindle power tool spindle stop",
	"M131": "Z axis synchronisation cancel",
	"M132": "Y1 axis mirror OFF",
	"M133": "SP2 forward and HP coolant ON",
	"M134": "SP2 reverse and HP coolant ON",
	"M135": "Left over catcher start",
	"M136": "Additional coolant pump ON",
	"M137": "Additional coolant pump OFF",
	"M138": "Additional coolant valve ON",
	"M139": "Additional coolant valve OFF",
	"M140": "Vibration stopper close (OP CAV)",
	"M141": "Drill block return",
	"M142": "Material clamp ON (OP CAV)",
	"M143": "Material clamp OFF (OP CAV)",
	"M144": "Channel lock & channel close (OP CAV)",
	"M145": "Channel lock & channel open (OP CAV)",
	"M146": "Bar lifter rise (OP CAV)",
	"M147": "Bar lifter descend (OP CAV)",
	"M148": "Push rod - air cylinder advance (OP CAV)",
	"M149": "Push rod - air cylinder retract (OP CAV)",
	"M151": "Gang post retract",
	"M152": "A7 axis excess error range expansion enabled (OP CAV)",
	"M153": "A7 axis excess error range expansion disabled (OP CAV)",
	"M154": "A7 axis servo ON (OP CAV)",
	"M155": "A7 axis servo OFF (OP CAV)",
	"M156": "SP2 work check",
	"M157": "Work ejector advance (option)",
	"M158": "Work ejector return (option)",
	"M159": "Work ejector cycle (option)",
	"M160": "S1 cutting start interlock ON",
	"M161": "S1 cutting start interlock OFF",
	"M162": "S2 cutting start interlock ON",
	"M163": "Coolant valve 1 OFF",
	"M164": "SP2 left hand rigid tap ON",
	"M165": "Coolant valve 2 OFF",
	"M166": "Coolant valve 3 ON",
	"M167": "Coolant valve 3 OFF",
	"M168": "Coolant valve 4 ON",
	"M169": "Coolant valve 4 OFF",
	"M170": "Index workpiece separator",
	"M171": "SP2 retract",
	"M174": "Optional subspindle power tool spindle forward",
	"M175": "Optional subspindle power tool spindle reverse",
	"M176": "Optional subspindle power tool spindle stop",
	"M177": "Option 5 air blow ON",
	"M178": "Limit RGB torque during acceleration / decelleration",
	"M179": "Cancel RGB torque during acceleration / decelleration",
	"M180": "T code duplication enabled",
	"M181": "SP2 Power tools reverse",
	"M182": "SP2 Power tools stop",
	"M190": "C1-C2 axis superimpose ON",
	"M191": "C1-C2 axis superimpose OFF",
	"M192": "Work basket advance",
	"M193": "Work basket retract",
	"M194": "Toolpost 2 retraction",
	"M195": "Step by step barchange",
	"M196": "SP2 rigid tap mode OFF",
	"M197": "SP2 ejector advance",
	"M198": "SP2 ejector retract",
	"M199": "Work basket swing down",
	"M200": "Phase adjustment value clear",
	"M201": "Phase adjustment value write",
	"M203": "SP1 and SP2 rpm forward synchro start",
	"M204": "SP1 and SP2 rpm reverse synchro start",
	"M205": "Thread Cutting & chamfering OFF",
	"M206": "SP1 and SP2 rpm synchro cancel",
	"M207": "Completion check of spindle synchronous",
	"M208": "Completion check of spindle 1 polygon synchronous",
	"M209": "Completion check of spindle 2 polygon synchronous",
	"M210": "SP1 and SP2 synchro phase hold",
	"M211": "SP1 and SP2 synchro phase cancels",
	"M212": "Y Mirror image OFF",
	"M213": "Parts conveyor ON",
	"M215": "Parts conveyor OFF",
	"M217": "Hand open for servo part catcher",
	"M218": "Hand close for servo part catcher",
	"M220": "$1 Tool monitor invalid",
	"M221": "$1 Tool monitor valid",
	"M222": "Loader retract",
	"M223": "Loader arm advance",
	"M224": "Loader arm retract",
	"M225": "C1/C2 axis synchro OFF",
	"M226": "SP1 C axis brake ON",
	"M227": "SP1 C axis brake OFF",
	"M228": "SP2 C axis brake ON",
	"M229": "SP2 C axis brake OFF",
	"M230": "Coolant valve 1 ON",
	"M231": "Z1/Z2 and Zs(B) axis swap control OFF",
	"M232": "Coolant valve 2 ON",
	"M233": "Z1 and B superimpose OFF",
	"M234": "Coolant valve 3 ON",
	"M235": "Coolant valve 3 OFF",
	"M236": "Coolant valve 4 ON",
	"M237": "C1/C2 axis mixture control OFF",
	"M238": "X1/Z1 and X2/Z2 axis synchro ON",
	"M239": "Back machining program skip",
	"M240": "SP2 rapid for pickoff",
	"M241": "Touch sensor drill breakage detection (2)",
	"M245": "Revolving tool stop - immediate completion",
	"M248": "Touch sensor check",
	"M249": "drill breakage alarm check",
	"M250": "Spare M250 code ON",
	"M251": "Z2 stroke extend OFF",
	"M252": "Spare M252 code ON",
	"M253": "Spare M253 code ON",
	"M254": "Drill checker 1 lever type start (op)",
	"M255": "Spare M255 code ON",
	"M256": "Turret needle touch type drill breakage detection operation ",
	"M258": "Turret 1 drill breakage detector teaching ",
	"M260": "Spare M250 code OFF",
	"M261": "External M61 relay OFF",
	"M262": "External M62 relay ON",
	"M263": "External M62 relay OFF",
	"M264": "External M63 relay ON",
	"M265": "Drill checker 2 home position check (op)",
	"M266": "External M64 relay ON",
	"M267": "External M64 relay OFF",
	"M268": "External M65 relay ON",
	"M269": "External M65 relay OFF",
	"M270": "Cut off confirmation (rotation formula)",
	"M271": "Cut off confirmation (servo formula)",
	"M272": "Cut off confirmation advance (cylinder)",
	"M273": "Cut off confirmation retract (cylinder)",
	"M274": "Part Separator Open (Option)",
	"M275": "Part Separator Close (Option)",
	"M276": "Cut off confirmation cycle",
	"M277": "Shutter open for servo parts catcher",
	"M278": "Shutter close for servo parts catcher",
	"M279": "Shutter close for servo parts catcher - finish",
	"M280": "Optional power tool spindle forward",
	"M281": "Optional power tool spindle reverse",
	"M282": "Optional power tool spindle stop",
	"M283": "Bar exchange",
	"M284": "SP1 speed integral control invalid",
	"M285": "SP1 speed integral control invalid cancel",
	"M286": "SP2 speed integral control invalid",
	"M287": "SP2 speed integral control invalid cancel",
	"M290": "B axis execution completion confirmation",
	"M291": "B axis start",
	"M293": "SP1 & SP2 synchro forward and coolant 1 & 2 start",
	"M294": "SP1 & SP2 synchro reverse and coolant 1 & 2 start",
	"M295": "SP1 & SP2 synchro stop and coolant 1 & 2 stop",
	"M297": "Sequence number search",
	"M298": "Jumps 1,2,3 & 4",
	"M299": "Eject check",
	"M300": "Torque limit cancel",
	"M301": "X axis torque limit command",
	"M302": "Z axis torque limit command",
	"M303": "Y axis torque limit command",
	"M304": "Multi Position workpiece separator index",
	"M305": "C axis torque limit command",
	"M306": "A axis torque limit command",
	"M310": "Torque limit non arrival check",
	"M311": "Torque limit arrived check",
	"M313": "Long workpiece rechuck",
	"M320": "Macro for subspindle mounted basket",
	"M321": "Parts catcher arm advance",
	"M322": "Parts catcher bucket advance",
	"M323": "Tool monitor valid",
	"M324": "Parts catcher arm retract",
	"M325": "Parts catcher bucket close",
	"M326": "4 division workpiece separator down",
	"M327": "4 division workpiece separator up",
	"M328": "HD3 turret HP coolant ON",
	"M329": "HD3 turret HP coolant OFF",
	"M330": "Work separator retract (conditional)",
	"M331": "Servo part catcher work pushing ON(M code)",
	"M332": "Servo part catcher work pushing ON(every time)",
	"M333": "Servo part catcher work pushing OFF check",
	"M334": "Servo part catcher moves forward to evacuation position",
	"M335": "Product unloading movement command",
	"M336": "Part catcher return",
	"M337": "Servo parts catcher one cycle operation",
	"M338": "HP option for inner spindle start",
	"M339": "HP option for inner spindle stop",
	"M340": "Workpiece separator sequence (conditional)",
	"M341": "SP2 to queue point (conditional)",
	"M344": "Product collection basket retreat",
	"M350": "Rapid feed rate setting ON",
	"M351": "Rapid feed rate setting reset",
	"M360": "Rapid deceleration time set",
	"M361": "Cancel rapid deceleration time set",
	"M362": "Z axis mirror image ON",
	"M363": "Z axis mirror image OFF",
	"M366": "Polygon machining ON subspindle tool post",
	"M377": "THY HD3 air blow ON",
	"M379": "THY HD3 air blow OFF",
	"M380": "Optional subspindle power tool spindle forward",
	"M381": "Optional subspindle power tool spindle reverse",
	"M382": "Optional subspindle power tool spindle stop",
	"M403": "SP1 & SP2 syncro forward start - complete",
	"M404": "SP1 & SP2 syncro reverse start - complete",
	"M408": "SP1 & SP2 syncro forward start - completion check",
	"M409": "SP1 & SP2 syncro reverse start - completion check",
	"M410": "X1/Z1 and X3/Z3 axis synchronous control ON",
	"M411": "X1/Z1 and X3/Z3 axis synchronous control OFF",
	"M412": "X2/Z2 and X3/Z3 axis synchronous control ON",
	"M413": "X3/Z3 and X3/Z3 axis synchronous control OFF",
	"M414": "Y1 & Y2 axis synchro ON",
	"M415": "Y1 & Y2 axis synchro OFF",
	"M416": "Y1 & Y3 axis synchro ON",
	"M417": "Y1 & Y3 axis synchro OFF",
	"M418": "Enable bar stock exchange",
	"M419": "Y2 & Y3 axis synchro OFF",
	"M420": "C2 & C3 mixture control ON",
	"M421": "C2 & C3 mixture control OFF",
	"M422": "C1 & C3 mixture control ON",
	"M423": "C1 & C3 mixture control OFF",
	"M424": "B1/B2(HD1,HD2) control ON",
	"M425": "B1/B2(HD1,HD2) control OFF",
	"M430": "Coolant valve 1 ON",
	"M431": "Coolant valve 1 OFF",
	"M432": "Coolant valve 2 ON",
	"M433": "Coolant valve 2 OFF",
	"M434": "Coolant valve 3 ON",
	"M435": "Coolant valve 3 OFF",
	"M436": "Coolant valve 4 ON",
	"M437": "Coolant valve 4 OFF",
	"M440": "C1 axis servo OFF",
	"M441": "C1 axis grid shift amount write",
	"M442": "C2 axis servo OFF",
	"M443": "C2 axis grid shift amount write",
	"M445": "Turret 1 revolving tool positioning stop high speed processing",
	"M447": "Parts catcher one cycle and check completion",
	"M451": "In position check invalid",
	"M452": "In position check valid",
	"M453": "Aux coolant pump OFF",
	"M455": "SP1 immediate stop",
	"M458": "Supply pump on",
	"M459": "Supply pump off",
	"M460": "High speed mode ON",
	"M461": "High speed mode OFF",
	"M465": "SP2 stop - immediate",
	"M475": "SP1 stop and coolant relation stop - immediate",
	"M485": "SP2 stop and coolant relation stop - immediate",
	"M493": "SP1 & SP2 synchro forward start & coolant start - immediate",
	"M494": "SP1 & SP2 synchro reverse start & coolant start - immediate",
	"M498": "SP1 & SP2 synchro forward start & coolant start",
	"M499": "SP1 & SP2 synchro reverse start & coolant start",
	"M500": "RVT positioning & Turret unclamp ",
	"M501": "Servo part catcher position 1-12",
	"M502": "Servo part catcher position 1-12",
	"M503": "Servo part catcher position 1-12",
	"M504": "Servo part catcher position 1-12",
	"M505": "Servo part catcher position 1-12",
	"M506": "Servo part catcher position 1-12",
	"M507": "Servo part catcher position 1-12",
	"M508": "Servo part catcher position 1-12",
	"M509": "Servo part catcher position 1-12",
	"M510": "Servo part catcher position 1-12",
	"M511": "Servo part catcher position 1-12",
	"M512": "Servo part catcher position 1-12",
	"M515": "Servo part catcher arm turn",
	"M516": "Servo part catcher arm return",
	"M521": "Servo part catcher feedrate",
	"M522": "Servo part catcher feedrate",
	"M523": "Servo part catcher feedrate",
	"M524": "Servo part catcher feedrate",
	"M525": "Servo part catcher feedrate",
	"M526": "Servo part catcher feedrate",
	"M527": "Servo part catcher feedrate",
	"M530": "Turret Index prohibit",
	"M531": "Turret Index prohibit cancel",
	"M535": "Tool counter command",
	"M540": "Rapid feedrate set",
	"M541": "Rapid feedrate set cancel",
	"M542": "Rapid feed acceleration rate set",
	"M543": "Rapid feed acceleration rate set cancel",
	"M545": "Revolving tool positioning high speed processing",
	"M580": "Spindle speed arrival level check changeable",
	"M599": "Lap time check",
	"M600": "Turret rotation prohibit",
	"M601": "Turret rotation prohibit cancel",
	"M602": "Servo part catcher position setting",
	"M603": "Servo part catcher position setting",
	"M604": "Servo part catcher position setting",
	"M605": "Servo part catcher position setting",
	"M606": "Servo part catcher position setting",
	"M607": "Servo part catcher position setting",
	"M608": "Servo part catcher position setting",
	"M609": "Servo part catcher position setting",
	"M610": "Servo part catcher position setting",
	"M611": "Servo part catcher position setting",
	"M612": "Servo part catcher position setting",
	"M620": "Thermal compensation start (X)",
	"M621": "Thermal compensation check (X)",
	"M622": "Thermal compensation system X1 shift",
	"M623": "Thermal compensation shift (X)",
	"M624": "Thermal compensation reset (Z)",
	"M625": "Thermal compensation start (Z)",
	"M626": "Outside work co-ordinate system shift check (Z)",
	"M627": "Outside work co-ordinate system shift (Z)",
	"M628": "Outside work co-ordinate system shift reset (Y)",
	"M629": "Outside work co-ordinate system shift start (Y)",
	"M630": "Outside work co-ordinate system shift check (Y)",
	"M631": "Outside work co-ordinate system shift (Y)",
	"M700": "Chuck open (Unconditional)",
	"M700-702": "Wait commands",
	"M780": "SP2 Indexing (zero point)",
	"M900-999": "Wait commands",
	"M1000-1255": "Spare M code (Binary output)",

}

let variableDefinitions = {};
let regions = [];

monaco.languages.register({ id: 'gcode' });

monaco.languages.setMonarchTokensProvider('gcode', {
		tokenizer: {
			root: [
				[/(IF|THEN|WHILE|GOTO)/, 'keyword'],
				[/(G|M)\d+/, 'identifier'],
				[/(\+|-|=|GT|LT|EQ|&&|AND|OR|\|\||!|NOT|==|!=|<|\<\=|>|\>=)/, 'operator'],
				[/(?<!(L|G|M))-?\d+(\.\d{1,2})?/, 'number'],
				[/[.]/, 'delimiter'],
				[/\#\d{1,3}/, 'variable'],
				[/\(([^)]+)?\)/, 'comment'],
			]
		},
	}
);

var model = monaco.editor.createModel('', 'gcode');
var handle = null;
model.onDidChangeContent(() => {
	validateModel(handle);
	handle = setTimeout(() => validateModel(), 500);
})

validateModel()

var editor = monaco.editor.create(document.getElementById('container'), {
	model,
	theme: 'vs',
	automaticLayout: true,
	cursorStyle:"block"
});

monaco.languages.registerFoldingRangeProvider('gcode', {
	provideFoldingRanges: function (model, context, token) {
		return regions;
	}
});


monaco.languages.registerHoverProvider("gcode", {
	provideHover: function (model, position) {
		// TODO check not within brackets 
		return {
			range: new monaco.Range(
				position.lineNumber,
				model.getWordAtPosition(position).startColumn,
				position.lineNumber,
				model.getWordAtPosition(position).endColumn
			),
			contents: getHover(model.getWordAtPosition(position).word),
		};
	},
});

function getHover(word) {
	if (word[0] === "G") {
		if (word.length === 3 && word[1] === "0") {
			word = word[0] + word[2];
		}
		let content = codeDefinitions[word];

		if (!content) return;

		return [
			{ value: "**" + word + "**" },
			{
				value:
					content.description,
			},
		];
	}
	else if (word[0] === "M") {
		if (word.length === 3 && word[1] === "0") {
			word = word[0] + word[2];
		}
		let content = codeDefinitions[word];

		if (!content) return;

		return [
			{ value: "**" + word + "**" },
			{
				value:
					content,
			},
		];
	}
	else if (word[0] === "#") {
		let content = variableDefinitions[word];

		if (!content) return;

		return [
			{ value: "**" + word + "**" },
			{
				value:
					content,
			},
		];
	}

	
}

monaco.languages.registerCompletionItemProvider('gcode', {
	provideCompletionItems: function (model, position) {

		var word = model.getWordUntilPosition(position);

		var range = {
			startLineNumber: position.lineNumber,
			endLineNumber: position.lineNumber,
			startColumn: word.startColumn,
			endColumn: word.endColumn
		};
		return {

			suggestions: createDependencyProposals(range)
		};

	},
	//triggerCharacters: ["#"]
});



function getSuggestions(word, range) {
	console.log("getSuggestions");
	console.log(word);

	if (!word) return [];

	let suggestions = [];
	if (word[0] === "#") {
		for (var kvp in variableDefinitions) {
			console.log(kvp);
			suggestions.push({
				label: variableDefinitions[kvp],
				kind: monaco.languages.CompletionItemKind.Function,
				insertText: kvp,
				range: range,
			},)
		}
	}

	return suggestions;
}

monaco.languages.setLanguageConfiguration('gcode',
	{
		autoClosingPairs: [{ open: '(', close: ')' }, { open: '[', close: ']' }],
		wordPattern: /(-?\d*\.\d\w*)|([^\`\~\!\@\%\#\^\&\*\(\)\-\=\+\[\{\]\}\\\|\;\:\'\"\,\.\<\>\/\?\s]+)|(\#\d+)/g
	});

function createDependencyProposals(range) {
	let suggestions = [];

	for (let i = 0; i < snippets.length; i++) {
		let x = snippets[i];
		x["range"] = range;
		suggestions.push(x);
	}

	for (var kvp in variableDefinitions) {
		let x = {
			label: variableDefinitions[kvp],
			kind: monaco.languages.CompletionItemKind.Function,
			insertText: kvp,
			range: range
		};

		suggestions.push(x);
	}

	return suggestions;
}

function validateModel() {
	console.log("validating...")

	let model = monaco.editor.getModels()[0];

	const markers = [];
	variableDefinitions = [];
	hints = [];

	let consecutiveEmptyLines = 0;
	let i = 1;
	while (i <= model.getLineCount()) {
		let line = model.getLineContent(i);

		let variablesOnLine = line.match(/#[0-9]+/);

		if (variablesOnLine) {
			if (line[0] === "#") {
				let name = variablesOnLine[0];
				let description = null;
				let bracketMatches = line.match(/\(([^)]+)\)/);
				if (bracketMatches) {
					description = bracketMatches[0].slice(1, -1).trim();
				}
				variableDefinitions[name] = description;
			}
			
			for (let j = 0; j < variablesOnLine.length; j++) {
				let hint = variableDefinitions[variablesOnLine[j]];
				if (!hint) continue;

				let pos = line.indexOf(variablesOnLine[j]) + 1;

				if (pos === 1) continue;

				hints.push({
					kind: monaco.languages.InlayHintKind.Type,
					position: { column: line.indexOf(variablesOnLine[j]) + 1 + variablesOnLine[j].length, lineNumber: i},
					label: `:${hint}`,
					whitespaceBefore: true,
				});
			}
		}

		if (line.trim() !== "") {
			if (consecutiveEmptyLines >= 3) {
				markers.push({
					source: "Lighthouse",
					message: "Maximum of two consecutive empty lines",
					severity: monaco.MarkerSeverity.Info,
					startLineNumber: i - consecutiveEmptyLines,
					startColumn: 1,
					endLineNumber: i,
					endColumn: 1,
					code:"lighthouse-01"
				});
			}

			if (line.startsWith("N") && consecutiveEmptyLines === 0) {
				markers.push({
					source: "Lighthouse",
					message: "Block should be preceeded by an empty line",
					severity: monaco.MarkerSeverity.Info,
					startLineNumber: i - consecutiveEmptyLines,
					startColumn: 1,
					endLineNumber: i,
					endColumn: line.length + 1,
					code: "lighthouse-05"
				});
			}

			consecutiveEmptyLines = 0;
		}
		else {
			consecutiveEmptyLines++;
		}

		


		if (line.endsWith(" ")) {
			let whitespaceStartsAt = line.length + 1;

			for (let c = line.length; c > 0; c--) {
				whitespaceStartsAt = c;
				if (line[c] !== " ") {
					break;
				}
			}


			markers.push({
				source: "Lighthouse",
				message: "Trailing whitespace",
				severity: monaco.MarkerSeverity.Info,
				startLineNumber: i,
				startColumn: whitespaceStartsAt +1,
				endLineNumber: i,
				endColumn: line.length + 1,
				code: "lighthouse-02"
			});
		}


		let gcodes = getGCodes(line);

		if (gcodes.length > 0) {

			let groups = [];

			for (let x = 0; x < gcodes.length; x++) {
				let code = gcodes[x];
				if (code.length === 3 && code[1] === "0") {
					code = code[0] + code[2];
				}

				let def = codeDefinitions[code];

				if (!def) {

					markers.push({
						source: "Lighthouse",
						message: "Unrecognised G Code",
						severity: monaco.MarkerSeverity.Error,
						startLineNumber: i,
						startColumn: line.indexOf(gcodes[x]) + 1,
						endLineNumber: i,
						endColumn: line.indexOf(gcodes[x]) + gcodes[x].length + 1,
						code: "lighthouse-03"
					});

					continue;
				}

				if (groups.includes(def.group)) { 
					markers.push({
						source: "Lighthouse",
						message: "G Codes from the same modal group are not permitted on the same line",
						severity: monaco.MarkerSeverity.Error,
						startLineNumber: i,
						startColumn: line.indexOf(gcodes[x]) + 1,
						endLineNumber: i,
						endColumn: line.indexOf(gcodes[x]) + gcodes[x].length + 1,
						code: "lighthouse-04"
					});
				}
				groups.push(def.group);
			}

		}

		i++;
	}

	monaco.editor.setModelMarkers(model, "owner", markers);
}


function getGCodes(line) {

	let matches = line.match(/G[0-9]{1,3}/g)
	return matches ? matches : [];
}

//monaco.languages.registerInlayHintsProvider("gcode", {
//	provideInlayHints(model, range, token) {
//		return {
//			hints,
//			dispose: () => { },
//		}
//	},
//});


let errorFixes = {
	"lighthouse-01": ""
};

let codeActionProvider = {
	provideCodeActions(model, range, context, token) {
		const actions = context.markers.map(error => {
			return {
				title: `Shorten to two line feeds`,
				diagnostics: [error],
				kind: "quickfix",
				edit: {
					edits: [
						{
							resource: model.uri,
							edit: {
								range: error,
								text: String.fromCharCode(13) + String.fromCharCode(13)
							}
						}
					]
				},
				isPreferred: true
			};
		});
		return {
			actions: actions,
			dispose: () => { }
		}
	}
}

monaco.languages.registerCodeActionProvider("gcode", codeActionProvider);


function setContent(content) {
	editor.setValue(content);
}

function pushSnippet(text, doc, toInsert) {
	snippets.push({
		label: text,
		kind: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
		documentation: doc,
		insertText: toInsert,
		insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
	});
}

function setTheme(themeData) {
	if (themeData === "vs-dark") {
		monaco.editor.setTheme("vs-dark");
		return;
	}

	if (themeData === "vs") {
		monaco.editor.setTheme("vs");
		return;
	}

	if (themeData === "hc-black") {
		monaco.editor.setTheme("hc-black");
		return;
	}

	if (themeData === "hc-light") {
		monaco.editor.setTheme("hc-light");
		return;
	}

	monaco.editor.defineTheme('monokai', themeData);
	monaco.editor.setTheme('monokai');
}
