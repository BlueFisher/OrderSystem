/* global angular */
/* global $ */
var app = angular.module('orderApp',[]);

app.directive('ngStaticHeight',function(){
	return function($scope,$elem,attr){
		$elem.height(
			$(window).height()-$('section.search').height()-$('section.search').height()
		);
	};
});