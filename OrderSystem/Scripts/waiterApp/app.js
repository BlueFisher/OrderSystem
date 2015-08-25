/* global angular */
/* global $ */

var app = angular.module('waiterApp', ['ngRoute', 'ui.bootstrap']);

app.config(function ($routeProvider) {
	$routeProvider.when('/', {
		templateUrl: 'Waiter/Partial/partial-index',
		controller: 'indexCtrl'
	}).when('/cart', {
		templateUrl: 'Waiter/Partial/partial-cart',
		controller: 'cartCtrl'
	}).when('/payment', {
		templateUrl: 'Waiter/Partial/partial-payment',
		controller: 'paymentCtrl'
	}).when('/success', {
		templateUrl: 'Waiter/Partial/partial-success',
		controller: 'successCtrl'
	})
});