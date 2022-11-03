

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
		label: '#region',
		kind: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
		documentation: 'Test.',
		insertText: 'region ${1:Region Name}\n\n\n#endregion',
		insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,

	},
	{
		label: '#endregion',
		kind: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
		documentation: 'Test.',
		insertText: 'endregion',

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

	{
		label: 'var',
		kind: monaco.languages.CompletionItemKind.Function,
		documentation: 'test',
		insertText: '#${1:var}=${2:0.0}\t(${3:DIM}=#${1})',
		insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,

	},
];

let regions = [];

monaco.languages.register({ id: 'gcode' });

// monaco.editor.defineTheme('lighthouseTheme', {
// 	base: 'vs-dark', // can also be vs or hc-black
// 	inherit: true,
// 	rules: [
// 		{ token: 'custom-region', foreground: 'c5e478', fontStyle: 'italic underline' },
// 		{ token: 'custom-var', foreground: 'c5e478' },
// 		{ token: 'custom-op', foreground: '7fdbca' },
// 		{ token: 'custom-logical', foreground: 'c792ea' },
// 		{ token: 'custom-command', foreground: 'c792ea' },
// 		{ token: 'custom-comment', foreground: '#637777' },
// 		{ token: 'custom-number', foreground: '#F78C6C' },

// 		// { token: 'comment', foreground: 'ff0044' },
// 	],
// 	colors: {
// 		'editor.foreground': '#d6deeb',
// 		'editor.background': '#011627',
// 		'editorCursor.foreground': '#80a4c2',
// 		'editor.lineHighlightBackground': '#0000FF20',
// 		'editorLineNumber.foreground': '#4b6479',
// 		'editor.selectionBackground': '#1d3b53',
// 		'editor.inactiveSelectionBackground': '#88000015'
// 	}
// });

// let keywords = ['IF', 'THEN', 'WHILE', 'GOTO'];
// let operators = [ 'LT', 'GT', 'EQ'];

monaco.languages.setMonarchTokensProvider('gcode', {

	tokenizer: {
		root: [
			[/\#(end)?region.*$/, 'entity.name.class'],
			
			[/(IF|THEN|WHILE|GOTO|GT|LT|EQ|&&|AND|OR|\|\||!|NOT|==|!=|<|\<\=|>|\>=)/, 'keyword'],

			[/(?<!L)-?\d+(\.\d{1,2})?/, 'constant.numeric'],
			[/\#\d{1,3}/, 'variable.other.readwrite.instance'],
			[/\(([^)]+)?\)/, 'comment'],

			[/(M|G|X|Y|Z|S|W|C|F|P|Q){1}/, 'support.function'],

		]
	},

});


var editor = monaco.editor.create(document.getElementById('container'), {
	value: `O104(SHL SCREW P0130 A4)
$1
(<MACHINEID>L62</MACHINEID>)
(P0130 A4 SHOULDER SCREWS)
(SIZES P0130.040 TO P0130.100 A4 ONLY)

(L32 PROGRAM ONLY)

(Lighthouse ID #104r2)

#region Setup

(TOOLS)
(T1 PART OFF 3.0MM WIDE)
(T2 SCREWCUT)
(T4 NOT USED)
(T3 FINISH TURN)
(T5 D1 UNDERCUT & L1 LENGTH  ONLY)

(T35 SUB FACE)
(T36 10MM SPOT)
(T37 HORN BROACH)
(T34 DRILL)
(T31 NOT USED)

#endregion

#511=8.0(D2=#511)
#512=10.0(D1=#512)
#513=14.0(D3=#513)
#514=12.0(L2=#514)
#515=50.0(L1=#515)
#516=7.0(H=#516)
#517=5.0(A/F=#517)
#518=0.025(L1 TOL)
#519=0.0125(D1 TOL)


(ADD-THIS-LINE-TO-OTHER-PRG)
(IN ORDER TO RUN SCHEDULE PROG)
IF[#17GT#0]THEN#515=#560


#520=[#515-#518]
#521=[#512-#519]

#522=5.0(BORE SIZE)
#523=5.03(A/F SIZE)
#524=2500(FEEDRATE/MIN)
#525=4.5(HEXAGON DEPTH)
#526=0.05(DEPTH OF CUT)
#527=6.0(NUMBER OF BROACHES)


!L1

IF[#599EQ1]GOTO99

(START UP)


#555=[#514+#520+#516](OAL)

M9
M52
G50Z[#500-#501]-#502
M6
M436(AGB CLOSED)

M350Z10000
M360X3Y3Z3
G13.1G18G97G99G113
M89M94M96M124
G0X#814+1.0Z-1.0

(CHECKS BAR HAS GONE)
M51

G630(MAIN/SUB SIMULTANEOUS)

(FACE + TURN FOR THREAD)
(HOLDER SDJCR1212-E11S)`,
	language: 'gcode',
	theme: "lighthouseTheme",
	automaticLayout: true,
});


monaco.languages.registerFoldingRangeProvider('gcode', {
	provideFoldingRanges: function (model, context, token) {
		return regions;
	}
});

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
	triggerCharacters: ["#"]
});


monaco.languages.setLanguageConfiguration('gcode', { autoClosingPairs: [{ open: '(', close: ')' }] });


function createDependencyProposals(range) {
	// returning a static list of proposals, not even looking at the prefix (filtering is done by the Monaco editor),
	// here you could do a server side lookup

	for(let i = 0; i < snippets.length; i++) {
		snippets[i]["range"] = range;
	}

	return snippets;
}



// G Code stuff
// monaco.languages.registerHoverProvider('gcode', {
// 	provideHover: function (model, position) {

// console.log(position)

// 			return {
// 				range: new monaco.Range(
// 					1,
// 					1,
// 					model.getLineCount(),
// 					model.getLineMaxColumn(model.getLineCount())
// 				),
// 				contents: [
// 					{ value: '**SOURCE**' },
// 					{ value: '```html\n' + '\n```' }
// 				]
// 			};
// 	}
// });


function setContent(content) {
	monaco.editor.getModels()[0].setValue(content);
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


// setInterval(detectRegions, 2000);

// function detectRegions() {
// 	console.log(regions.length, "regions exist")


// 	let model = monaco.editor.getModels()[0]
// 	let newRegions = [];
// 	let regionStarts = null;
// 	for(i = 0; i < model.getLineCount(); i++) {
// 		let line = model.getLineContent(i+1);
// 		if(line.startsWith("#region")) {
// 			regionStarts = i+1;
// 		}

// 		if(line.startsWith("#endregion") && regionStarts) {
// 			newRegions.push({
// 				start: regionStarts,
// 				end: i+1,
// 				kind: monaco.languages.FoldingRangeKind.Region
// 			})
// 			regionStarts = null;
// 		}
// 	}
// 	regions = newRegions;

// 	monaco.languages.registerFoldingRangeProvider('gcode', {
// 		provideFoldingRanges: function (model, context, token) {
// 			return regions;
// 		}
// 	});

	
// }

function setTheme(themeData) {
	monaco.editor.defineTheme('monokai', themeData);
	monaco.editor.setTheme('monokai');
}

setTheme("Cobalt2")


// setContent("Hello, world");

// var originalModel = monaco.editor.createModel('#511=8.0(D2=#511)\n(Test)', 'gcode');
// var modifiedModel = monaco.editor.createModel('#511=12.0(D2=#511)\n(Test)\n(Test)', 'gcode');

// var diffEditor = monaco.editor.createDiffEditor(document.getElementById('container'));
// diffEditor.setModel({
// 	original: originalModel,
// 	modified: modifiedModel,
// });

// diffEditor.updateOptions({ theme: 'vs-dark', automaticLayout: true });
