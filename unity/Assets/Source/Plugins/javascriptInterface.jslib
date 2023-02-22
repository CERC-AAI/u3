

mergeInto(LibraryManager.library, {

  SendMessageToJavascript: function (func, arguments) {  
	func = Pointer_stringify(func);
	arguments = Pointer_stringify(arguments);
  
    console.log(func + ": " + arguments);
	
	var fn = window[func];
	
	if (typeof fn === "function")
	{
		fn.apply(null, [arguments]);
	}
	else
	{
		console.log("Function " + func + "not defined in global scope.");
	}
  },

});

