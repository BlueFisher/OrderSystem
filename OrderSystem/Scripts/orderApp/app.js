/* global angular */
/* global $ */

var app = angular.module('orderApp', ['ngRoute', 'ui.bootstrap']);

app.config(function ($routeProvider) {
	
    $routeProvider
		.when('/', {
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
	}).when('/error', {
		templateUrl: 'Home/Partial/partial-error',
		controller: 'errorCtrl'
	});
});



app.controller('cartCtrl', [
	'$scope',
	'$rootScope',
	'$filter',
	'$location',
	'statusRemain',
	'generateMenuSubClassPromise',
	'generateMenuDetailPromise',
	'generateRemarkPromise',
	function ($scope, $rootScope, $filter, $location, statusRemain, GMSCP, GMDP, GRP) {
		$rootScope.hideNavBar = true;
		if ($rootScope.cart == undefined) {
			$rootScope.cart = {
				isInitialized: false,
				sizeAll: 0,
				priceAll: 0,
				results: [],
				table: null,
				customer: 1,
				bill: ''
			};
		}
		var rootCart = $rootScope.cart;

		var param = $location.search();
		if (param.table == undefined) {
			$location.path('/');
			$location.search('table', '1');
		} else {
			rootCart.table = parseInt(param.table);
		}

		var menuDetail;
		GMSCP.then(function (classes) {
			$scope.menuSubClass = classes;

			if (statusRemain.activeClass == null) {
				statusRemain.activeClass = $scope.menuSubClass[0];
			}
			_changeMode(classMode.rank);

			GMDP.then(function (menus) {
				menuDetail = menus;

				GRP.then(function (notes) {
					if (!rootCart.isInitialized) {
						for (var i = 0; i < menuDetail.length; i++) {
							menuDetail[i].cart.filteredNotes = angular.copy(notes);
						}
					}
					_filterMenu();
					rootCart.isInitialized = true;
				});
			});
		});

		var classMode = {
			search: 0,
			normal: 1,
			rank: 2
		};

		$scope.isRankMode = function () {
			return $scope.currentMode == classMode.rank;
		};
		$scope.isSearchMode = function () {
			return $scope.currentMode == classMode.search;
		};
		function _changeMode(mode) {
			$scope.currentMode = mode;
			if (mode != classMode.search) {
				$scope.searchText = '';
			}
			if (mode != classMode.normal) {
				statusRemain.activeClass.cart.isSelected = false;
			}
		}

		// click subclass
		$scope.toggleSelected = function (c) {
			// enter the normalMode
			_changeMode(classMode.normal);

			statusRemain.activeClass.cart.isSelected = false;
			c.cart.isSelected = true;
			statusRemain.activeClass = c;
			_filterMenu();
		};

		$scope.enterRankMode = function () {
			_changeMode(classMode.rank);
			_filterMenu();
		};

		$scope.searchChange = function (searchText) {
			if (searchText == '') {
				// exit the search mode
				_changeMode(classMode.rank);
				_filterMenu();
			} else {
				// enter the search mode
				_changeMode(classMode.search);
				_filterMenu(searchText);
			}
		};
		
		// add and remove Meal
		$rootScope.addMenu = function (menu) {
			menu.cart.ordered++;
			rootCart.sizeAll++;
			rootCart.priceAll += menu.DisherPrice;
			for (var i = 0; i < $scope.menuSubClass.length; i++) {
				if ($scope.menuSubClass[i].SubClassId == menu.DisherSubclassID1) {
					$scope.menuSubClass[i].cart.ordered++;
					break;
				}
			}
			if (menu.cart.ordered == 1) {
				rootCart.results.push(menu);
			}
		};
		$rootScope.removeMenu = function (menu) {
			menu.cart.ordered--;
			rootCart.sizeAll--;
			rootCart.priceAll -= menu.DisherPrice;
			for (var i = 0; i < $scope.menuSubClass.length; i++) {
				if ($scope.menuSubClass[i].SubClassId == menu.DisherSubclassID1) {
					$scope.menuSubClass[i].cart.ordered--;
					break;
				}
			}
			if (menu.cart.ordered == 0) {
				for (var i = 0; i < rootCart.results.length; i++) {
					if (angular.equals(menu, rootCart.results[i])) {
						rootCart.results.splice(i, 1);
						break;
					}
				}
			}
		};
		// add and remove Remark
		$rootScope.addNote = function (menu, note, index) {
			menu.cart.notes.push(note);
			menu.cart.filteredNotes.splice(index, 1);
		};
		$rootScope.removeNote = function (menu, note, index) {
			menu.cart.filteredNotes.push(note);
			menu.cart.notes.splice(index, 1);
		};


		function _filterMenu(searchText) {
			var filteredArr = menuDetail;

			switch ($scope.currentMode) {
				case classMode.normal:
					filteredArr = $filter('filter')(filteredArr, {
						DisherSubclassID1: statusRemain.activeClass.SubClassId
					}, true);
					filteredArr = $filter('orderBy')(filteredArr, '-DisherPoint');
					break;
				case classMode.rank:
					filteredArr = $filter('orderBy')(filteredArr, '-DisherPoint');
					filteredArr = $filter('limitTo')(filteredArr, 10);
					break;
				case classMode.search:
					filteredArr = $filter('filter')(menuDetail, searchText);
					break;
			}

			$scope.filteredMenuDetail = filteredArr;
		}
	}
]).controller('resultCtrl', [
	'$scope',
	'$rootScope',
	'$location',
	function ($scope, $rootScope, $location) {
		$rootScope.hideNavBar = false;
		$rootScope.viewTitle = "查看订单"
		if ($rootScope.cart == null) {
			$location.path('/');
			$location.search('table', '1');
			return;
		}
		
		$rootScope.submit = function () {
			console.log($rootScope.cart);
		};

		angular.forEach($rootScope.cart.results, function (menu) {
			menu.cart.isNoteCollapsed = false;
		});
	}
]).controller('paymentCtrl', [
	'$scope',
	'$rootScope',
	'$location',
	function ($scope, $rootScope, $location) {
		$rootScope.viewTitle = "结算"
		if ($rootScope.cart == null) {
			// $location.path('/');
			// $location.search('table', '1');
			// return;
			$rootScope.cart = {
				isInitialized: true,
				sizeAll: 5,
				priceAll: 50,
				results: [],
				table: 10,
				customer: 1,
				bill: ''
			};
		}
	}
]).controller('signinCtrl', [
	'$scope',
	'$rootScope',
	'$location',
	function ($scope, $rootScope, $location) {
		$rootScope.viewTitle = "登录"
		if ($rootScope.cart == null) {
			// $location.path('/');
			// $location.search('table', '1');
			// return;
			$rootScope.cart = {
				isInitialized: true,
				sizeAll: 5,
				priceAll: 50,
				results: [],
				table: 10,
				customer: 1,
				bill: ''
			};
		}
	}
]).controller('signinCtrl', [
	'$scope',
	'$rootScope',
	'$location',
	function ($scope, $rootScope, $location) {
		$rootScope.viewTitle = "注册"
		if ($rootScope.cart == null) {
			// $location.path('/');
			// $location.search('table', '1');
			// return;
			$rootScope.cart = {
				isInitialized: true,
				sizeAll: 5,
				priceAll: 50,
				results: [],
				table: 10,
				customer: 1,
				bill: ''
			};
		}
	}
]);