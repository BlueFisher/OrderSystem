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
		}).when('/client', {
			templateUrl: 'Home/Partial/partial-client',
			controller: 'clientCtrl'
		}).when('/error', {
			templateUrl: 'Home/Partial/partial-error',
			controller: 'errorCtrl'
		});
});


app.controller('accountCtrl', [
	'$scope',
	'$rootScope',
	'isAuthenticated',
	'$http',
	function ($scope, $rootScope, IAed, $http) {
		$rootScope.refreshClient = function () {
			IAed().then(function (data) {
				if (data.IsSucceed) {
					$scope.isAuthenticated = true;
					$scope.clientName = data.Addition.Name;
				} else {
					$scope.isAuthenticated = false;
				}
			});
		}

		$scope.isAuthenticated = false;
		$rootScope.refreshClient();

		$scope.signout = function () {
			$http.post('/Account/Signout').success(function (data) {
				if (data.IsSucceed) {
					$rootScope.refreshClient();
				}
			})
		}
	}
]);


app.controller('cartCtrl', [
	'$scope',
	'$rootScope',
	'$filter',
	'$location',
	'statusRemain',
	'getTable',
	'generateMenuSubClassPromise',
	'generateMenuDetailPromise',
	'generateRemarkPromise',
	function ($scope, $rootScope, $filter, $location, statusRemain, GT, GMSCP, GMDP, GRP) {
		$rootScope.viewTitle = '菜单';
		$rootScope.hideBackBtn = true;
		if ($rootScope.cart == undefined) {
			$rootScope.cart = {
				IsInitialized: false,
				SizeAll: 0,
				PriceAll: 0,
				Results: [],
				Table: null,
				Customer: 1,
				Bill: '',
				IsPaid: 0,
				PayKind: null
			};
		}
		var rootCart = $rootScope.cart;

		var param = $location.search();
		if (param.qrCode == undefined) {
			$location.path('/');
			$location.search('qrCode', '101');
		} else {
			GT(param.qrCode).then(function (deskInfo) {
				rootCart.Table = deskInfo;
			});
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
					if (!rootCart.IsInitialized) {
						for (var i = 0; i < menuDetail.length; i++) {
							menuDetail[i].Additional.FilteredNotes = angular.copy(notes);
						}
					}
					_filterMenu();
					rootCart.IsInitialized = true;
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
				statusRemain.activeClass.Additional.IsSelected = false;
			}
		}

		// click subclass
		$scope.toggleSelected = function (c) {
			// enter the normalMode
			_changeMode(classMode.normal);

			statusRemain.activeClass.Additional.IsSelected = false;
			c.Additional.IsSelected = true;
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
			menu.Additional.Ordered++;
			rootCart.SizeAll++;
			rootCart.PriceAll += menu.DisherPrice;
			for (var i = 0; i < $scope.menuSubClass.length; i++) {
				if ($scope.menuSubClass[i].SubClassId == menu.DisherSubclassID1) {
					$scope.menuSubClass[i].Additional.Ordered++;
					break;
				}
			}
			if (menu.Additional.Ordered == 1) {
				rootCart.Results.push(menu);
			}
		};
		$rootScope.removeMenu = function (menu) {
			menu.Additional.Ordered--;
			rootCart.SizeAll--;
			rootCart.PriceAll -= menu.DisherPrice;
			for (var i = 0; i < $scope.menuSubClass.length; i++) {
				if ($scope.menuSubClass[i].SubClassId == menu.DisherSubclassID1) {
					$scope.menuSubClass[i].Additional.Ordered--;
					break;
				}
			}
			if (menu.Additional.Ordered == 0) {
				for (var i = 0; i < rootCart.Results.length; i++) {
					if (angular.equals(menu, rootCart.Results[i])) {
						rootCart.Results.splice(i, 1);
						break;
					}
				}
			}
		};
		// add and remove Remark
		$rootScope.addNote = function (menu, note, index) {
			menu.Additional.Notes.push(note);
			menu.Additional.FilteredNotes.splice(index, 1);
		};
		$rootScope.removeNote = function (menu, note, index) {
			menu.Additional.FilteredNotes.push(note);
			menu.Additional.Notes.splice(index, 1);
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
		$rootScope.viewTitle = '查看订单';
		$rootScope.hideBackBtn = false;
		if ($rootScope.cart == null) {
			$location.path('/');
			return;
		}

		angular.forEach($rootScope.cart.Results, function (menu) {
			menu.Additional.IsNoteCollapsed = false;
		});
	}
]).controller('paymentCtrl', [
	'$scope',
	'$rootScope',
	'$location',
	'$http',
	function ($scope, $rootScope, $location, $http) {
		$rootScope.viewTitle = '结算';
		$rootScope.hideBackBtn = false;
		if ($rootScope.cart == null) {
			$location.path('/');
			return;
		}
		$scope.customersAll = [];
		for(var i=0;i<50;i++){
			$scope.customersAll.push(i+1);
		}
		$scope.submit = function () {
			$http.post('/Cart/Submit', $rootScope.cart).success(function (data) {
				alert('已提交订单成功，请服务员收费')
			});
		};
		$scope.hasPayName = false;
		$scope.pay = function () {
			$http.post('/Cart/GetPayName').success(function (data) {
				$scope.hasPayName = true;
				$scope.pays = data;
			});
		}
		$scope.netPay = function(pay){
			$rootScope.cart.IsPaid = 1;
			$rootScope.cart.PayKind = pay.PayName;
			$http.post('/Cart/Submit', $rootScope.cart).success(function (data) {
				alert('已支付成功');
			});
		}
	}
]);

app.controller('signinCtrl', [
	'$scope',
	'$rootScope',
	'$http',
	function ($scope, $rootScope, $http) {
		$rootScope.viewTitle = '登录';
		$rootScope.hideBackBtn = false;

		$scope.signinFormData = {};
		$scope.signin = function () {
			$http.post('/Account/Signin', $scope.signinFormData).success(function (data) {
				if (data.IsSucceed) {
					$rootScope.refreshClient();
					history.back();
				} else {
					alert(data.ErrorMessage);
				}
			});
		}
	}
]).controller('signupCtrl', [
	'$scope',
	'$rootScope',
	'$http',
	'$interval',
	function ($scope, $rootScope, $http, $interval) {
		$rootScope.viewTitle = '注册';
		$rootScope.hideBackBtn = false;

		$scope.signupFormData = {};
		$scope.isSendSMS = false;
		$scope.canSendSMS = true;
		$scope.sendSMSBtnText = '发送验证码';

		$scope.sendSMS = function () {
			$http.post('/Account/SendSMS', {
				Mobile: $scope.signupFormData.Mobile
			}).success(function (data) {
				if (data.IsSucceed) {
					$scope.isSendSMS = true;
					$scope.canSendSMS = false;

					var interval = 60;
					var timer = $interval(function () {
						$scope.sendSMSBtnText = interval + '秒后重发';
						interval--;
						if (interval == 0) {
							$scope.canSendSMS = true;
							$scope.sendSMSBtnText = '重发';
							interval = 60;
							$interval.cancel(timer);
						}
					}, 1000);
				} else {
					alert(data.ErrorMessage);
				}
			});
		}

		$scope.signup = function () {
			$http.post('/Account/Signup', $scope.signupFormData).success(function (data) {
				if (data.IsSucceed) {
					$rootScope.refreshClient();

				} else {
					alert(data.ErrorMessage);
				}
			});
		}
	}
]);