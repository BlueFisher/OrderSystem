﻿/* global angular */
/* global $ */

var app = angular.module('orderApp', ['ngRoute', 'ui.bootstrap']);

app.config(function ($routeProvider) {
    $routeProvider.when('/', {
		templateUrl: 'Home/Partial/partial-cart',
		controller: 'cartCtrl'
	}).when('/result', {
		templateUrl: 'Home/Partial/partial-result',
		controller: 'resultCtrl'
	}).when('/payment', {
		templateUrl: 'Home/Partial/partial-payment',
		controller: 'paymentCtrl'
	}).when('/signin', {
		templateUrl: 'Home/Partial/partial-signin',
		controller: 'signinCtrl'
	}).when('/signup', {
		templateUrl: 'Home/Partial/partial-signup',
		controller: 'signupCtrl'
	}).when('/client', {
		templateUrl: 'Home/Partial/partial-client',
		controller: 'clientCtrl'
	}).when('/privilege', {
		templateUrl: 'Home/Partial/partial-privilege',
		controller: 'privilegeCtrl'
	}).when('/forget', {
		templateUrl: 'Home/Partial/partial-forget',
		controller: 'forgetCtrl'
	}).when('/error', {
		templateUrl: 'Home/Partial/partial-error',
		controller: 'errorCtrl'
	});
});
