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
		insertText: 'N301(M5 THREAD)\nX6.0\nG76X4.019Z#514+1.5P0.491Q0.2F0.8\nGOTO6',
		insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,

	},

	{
		label: 'M5 Thread',
		kind: monaco.languages.CompletionItemKind.Function,
		documentation: 'test',
		insertText: 'N301(M5 THREAD)\nX6.0\nG76X4.019Z#514+1.5P0.491Q0.2F0.8\nGOTO6',
		insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,

	},
];

let regions = [];

monaco.languages.register({ id: 'gcode' });

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
	value: '',
	language: 'gcode',
	theme: "Cobalt2",
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
	for(let i = 0; i < snippets.length; i++) {
		snippets[i]["range"] = range;
	}

	return snippets;
}


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




function setTheme(themeData) {
	monaco.editor.defineTheme('monokai', themeData);
	monaco.editor.setTheme('monokai');
}

setTheme("Cobalt2")