/* global angular */

app.controller('cartCtrl', [
	'$scope',
	'$rootScope',
	'$filter',
	'$location',
	'getTable',
	'generateMenuSubClassPromise',
	'generateMenuDetailPromise',
	'generateRemarkPromise',
	function ($scope, $rootScope, $filter, $location, GT, GMSCP, GMDP, GRP) {
		// test! if the url doesn't have qrCode data then redirect to the one has qrCode = 101
		if ($location.search().qrCode == undefined) {
			$location.search('qrCode', '101');
			return;
		}
		
		$rootScope.viewTitle = '菜单';
		$rootScope.hideBackBtn = true;

		var classMode = {
			search: 0,
			normal: 1,
			rank: 2,
			result: 3
		};
		var rootCart; // The abbr. of  $rootScope.cart
		var activeClass; // The current selected class
		
		if ($rootScope.cart == null) {
			$rootScope.cart = {
				SizeAll: 0,
				PriceAll: 0,
				Results: [],
				Table: null,
				Customer: 1,
				Bill: '',
				IsPaid: 0,
				PayKind: null
			};
			rootCart = $rootScope.cart;
			$rootScope.menuSubClass = null // The root data that contains subclass information
			$rootScope.menuDetail = null //The root data that contains menu information

			// Get Table Info
			GT($location.search().qrCode).then(function (deskInfo) {
				rootCart.Table = deskInfo;
			});
			
			// GetMenuSubClass
			GMSCP().then(function (classes) {
				$rootScope.menuSubClass = classes;
				activeClass = classes[0];
				
				_changeMode(classMode.rank);
				
				// GetMenuDetail
				GMDP().then(function (menus) {
					$rootScope.menuDetail = menus;
					_filterMenu();
					// GetRemark
					GRP().then(function (notes) {
						for (var i = 0; i < $rootScope.menuDetail.length; i++) {
							$rootScope.menuDetail[i].Additional.FilteredNotes = angular.copy(notes);
						}
						
					});
				});
			});
		} else {
			// If the $rootScope is defined
			rootCart = $rootScope.cart;
			activeClass = $rootScope.menuSubClass[0];
			
			_changeMode(classMode.rank);
			_filterMenu();
		}
		
		$scope.isRankMode = function () {
			return $scope.currentMode == classMode.rank;
		};
		$scope.isSearchMode = function () {
			return $scope.currentMode == classMode.search;
		};
		$scope.isResultMode = function(){
			return $scope.currentMode == classMode.result;
		}
		function _changeMode(mode) {
			$scope.currentMode = mode;
			if (mode != classMode.search) {
				$scope.searchText = '';
			}
			if (mode != classMode.normal) {
				activeClass.Additional.IsSelected = false;
			}
		}

		// click subclass
		$scope.toggleSelected = function (c) {
			// enter the normalMode
			_changeMode(classMode.normal);

			activeClass.Additional.IsSelected = false;
			c.Additional.IsSelected = true;
			activeClass = c;
			_filterMenu();
		};

		$scope.enterRankMode = function () {
			_changeMode(classMode.rank);
			_filterMenu();
		};
		$scope.enterResultMode = function(){
			_changeMode(classMode.result);
			_filterMenu();
		}

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
			var filteredArr = $rootScope.menuDetail;

			switch ($scope.currentMode) {
				case classMode.normal:
					filteredArr = $filter('filter')(filteredArr, {
						DisherSubclassID1: activeClass.SubClassId
					}, true);
					filteredArr = $filter('orderBy')(filteredArr, '-DisherPoint');
					break;
				case classMode.rank:
					filteredArr = $filter('orderBy')(filteredArr, '-DisherPoint');
					filteredArr = $filter('limitTo')(filteredArr, 10);
					break;
				case classMode.search:
					filteredArr = $filter('filter')($rootScope.menuDetail, searchText);
					break;
				case classMode.result:
					filteredArr = rootCart.Results
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
		
		// unselect all the classes
		angular.forEach($rootScope.menuSubClass, function (menu) {
			menu.Additional.IsSelected = false;
		});
		
		$scope.customersAll = [];
		for (var i = 0; i < 50; i++) {
			$scope.customersAll.push(i + 1);
		}
		$http.post('/Cart/GetPayName').success(function (data) {
			$scope.pays = data;
		});
		$http.post('/Cart/GetTablewareFeeFee').success(function(data){
			$scope.tablewareFee = parseInt(data.TablewareFee);
		});
		
		$scope.offlinePay = function () {
			$rootScope.cart.PriceAll += $rootScope.cart.Customer * $scope.tablewareFee;
			$http.post('/Cart/Submit', $rootScope.cart).success(function (data) {
				alert('已提交订单成功，请服务员收费');
				delete $rootScope.cart;
				$location.path('/client');
			});
		};
		
		$scope.onlinePay = function (pay) {
			$rootScope.cart.IsPaid = 1;
			$rootScope.cart.PayKind = pay.PayName;
			$rootScope.cart.PriceAll += $rootScope.cart.Customer * $scope.tablewareFee;
			$http.post('/Cart/Submit', $rootScope.cart).success(function (data) {
				alert('已支付成功');
				delete $rootScope.cart;
				$location.path('/client');
			});
		}
	}
]);