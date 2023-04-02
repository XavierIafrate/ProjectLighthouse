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
}

let variableDefinitions = {};

let regions = [];
monaco.languages.register({ id: 'gcode' });

monaco.languages.setMonarchTokensProvider('gcode', {
	tokenizer: {
		root: [
			[/(\+|-|=|GT|LT|EQ|&&|AND|OR|\|\||!|NOT|==|!=|<|\<\=|>|\>=)/, 'operator'],
			[/(?<!(L|G|M))-?\d+(\.\d{1,2})?/, 'constant.numeric'],
			[/\#\d{1,3}/, 'variable'],
			[/\(([^)]+)?\)/, 'comment'],
			[/(IF|THEN|WHILE|GOTO|[A-Z]{1}|(G|M)\d+)/, 'keyword'],
		]
	},
});

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
					severity: monaco.MarkerSeverity.Warning,
					startLineNumber: i - consecutiveEmptyLines,
					startColumn: 1,
					endLineNumber: i,
					endColumn: 1,
					code:"lighthouse-01"
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
				severity: monaco.MarkerSeverity.Warning,
				startLineNumber: i,
				startColumn: whitespaceStartsAt +1,
				endLineNumber: i,
				endColumn: line.length + 1,
				code: "lighthouse-02"
			});
		}


		let gcodes = getGCodes(line);

		if (gcodes.length > 1) {

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
						severity: monaco.MarkerSeverity.error,
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
						severity: monaco.MarkerSeverity.error,
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

	let matches = line.match(/G[0-9]{1,2}/g)
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
