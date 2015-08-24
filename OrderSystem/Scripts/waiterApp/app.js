/* global angular */
/* global $ */

var app = angular.module('waiterApp', ['ngRoute', 'ui.bootstrap']);

app.config(function ($routeProvider) {
	$routeProvider.when('/', {
		templateUrl: 'Waiter/Partial/partial-cart',
		controller: 'cartCtrl'
	})
});

app.controller('cartCtrl', function ($scope, $http) {
	$http.post('/Home/GetMenuDetail').success(function (data) {
		$scope.menuDetails = data;
	})
});