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

let regions = [];
monaco.languages.register({ id: 'gcode' });

monaco.languages.setMonarchTokensProvider('gcode', {

	tokenizer: {
		root: [
			[/\#(end)?region.*$/, 'entity.name.class'],
			[/(\+|-|=|GT|LT|EQ|&&|AND|OR|\|\||!|NOT|==|!=|<|\<\=|>|\>=)/, 'operator'],


			[/(?<!L)-?\d+(\.\d{1,2})?/, 'constant.numeric'],
			[/\#\d{1,3}/, 'variable'],
			[/\(([^)]+)?\)/, 'comment'],

			[/(IF|THEN|WHILE|GOTO)/, 'keyword'],
			[/(A|T|M|G|X|Y|Z|S|W|C|F|P|Q){1}/, 'keyword'],

		]
	},

});

var editor = monaco.editor.createDiffEditor(
	document.getElementById("container"),
	{
		enableSplitViewResizing: false,
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
	for (let i = 0; i < snippets.length; i++) {
		snippets[i]["range"] = range;
	}

	return snippets;
}

function setContent(content1, content2) {
	var originalModel = monaco.editor.createModel(content1, "gcode");
	var modifiedModel = monaco.editor.createModel(content2, "gcode");

	editor.setModel({
		original: originalModel,
		modified: modifiedModel,
	});
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
	monaco.editor.defineTheme('monokai', themeData);
	monaco.editor.setTheme('monokai');
}
