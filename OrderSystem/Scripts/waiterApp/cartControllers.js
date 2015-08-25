/* global app */
/* global angular */

app.controller('indexCtrl', [
	'$scope',
	'$rootScope',
	'$location',
	'$http',
	function ($scope, $rootScope, $location,$http) {
		$scope.form = {
			username: '',
			password:''
		}
		$scope.submit = function () {
			$http.post('/Waiter/Signin', $.param($scope.form), {
				headers: {
					'Content-Type': 'application/x-www-form-urlencoded;charset=utf-8'
				}
			}).success(function (data) {
				if (data.IsSucceed) {
					$location.path('/cart');
				} else {
					alert(data.ErrorMessage);
				}
			});
		}
	}
]);


app.controller('cartCtrl', [
	'$scope',
	'$rootScope',
	'$filter',
	'$location',
	'generateMenuSubClassPromise',
	'generateMenuDetailPromise',
	'generateRemarkPromise',
	'generateSetMeal',
	'$http',
	function ($scope, $rootScope, $filter, $location, GMSCP, GMDP, GRP, GSM, $http) {
		$http.post('/Waiter/IsAuthenticated').success(function (data) {
			if (!data.IsSucceed) {
				$location.path('/');
			}
		});


		var classMode = {
			search: 0,
			normal: 1,
			rank: 2,
			result: 3,
			onSale: 4
		};
		var rootCart; // The abbr. of  $rootScope.cart
		var activeClass; // The current selected class

		if ($rootScope.cart == null) {
			$rootScope.cart = {
				SizeAll: 0,
				PriceAll: 0,
				Results: [],
				Table: null,
				Customer: 0,
				Bill: '',
				IsPaid: 0,
				PayKind: null
			};
			rootCart = $rootScope.cart;
			$rootScope.menuSubClass = null // The root data that contains subclass information
			$rootScope.menuDetail = null //The root data that contains menu information

			// GetMenuSubClass
			GMSCP().then(function (classes) {
				$rootScope.menuSubClass = classes;
				activeClass = classes[0];

				_changeMode(classMode.rank);

				// GetMenuDetail
				GMDP().then(function (menus) {
					$rootScope.menuDetail = menus;
					GSM().then(function (list) {
						for (var i = 0; i < $rootScope.menuDetail.length; i++) {
							if ($rootScope.menuDetail[i].IsOnSale) {
								$rootScope.menuDetail[i].DisherPrice = parseFloat($rootScope.menuDetail[i].DisherVipPrice);
								$rootScope.menuDetail[i].DisherDiscount = 1;
							}
							if ($rootScope.menuDetail[i].IsSetMeal) {
								$rootScope.menuDetail[i].Additional.SetMealList = [];
								for (var j = 0; j < list.length; j++) {
									if (list[j].DisherSetId == $rootScope.menuDetail[i].DisherId) {
										for (var k = 0; k < $rootScope.menuDetail.length; k++) {
											if (list[j].DisherId == $rootScope.menuDetail[k].DisherId) {
												$rootScope.menuDetail[i].Additional.SetMealList.push($rootScope.menuDetail[k]);
											}
										}
										
									}
								}
								console.log($rootScope.menuDetail[i].Additional)
							}
							
						}
						
					});
					_filterMenu();
					// GetRemark
					GRP().then(function (notes) {
						for (var i = 0; i < $rootScope.menuDetail.length; i++) {
							$rootScope.menuDetail[i].Additional.FilteredNotes = angular.copy(notes);
						}
						_hasHistoryCart();

					});
				});
			});
		} else {
			// If the $rootScope is defined
			rootCart = $rootScope.cart;
			activeClass = $rootScope.menuSubClass[0];

			_changeMode(classMode.rank);
			_filterMenu();

			_hasHistoryCart();
		}

		function _hasHistoryCart() {
			if ($rootScope.historyCart != null) {
				$rootScope.cart.Customer = $rootScope.historyCart.Customer;

				for (var i = 0; i < $rootScope.historyCart.Results.length; i++) {
					for (var j = 0; j < $rootScope.menuDetail.length; j++) {
						if ($rootScope.historyCart.Results[i].DisherId == $rootScope.menuDetail[j].DisherId) {
							for (var k = 0; k < $rootScope.historyCart.Results[i].Ordered; k++) {
								$rootScope.addMenu($rootScope.menuDetail[j]);
							}
							break;
						}
					}
				}
			}
			$rootScope.historyCart = null;
		}

		$scope.showModal = function (menu) {
			var modalInstance = $modal.open({
				animation: $scope.animationsEnabled,
				templateUrl: 'myModalContent.html',
				controller: 'ModalInstanceCtrl',
			});

			$rootScope.currentImgSrc = menu.DisherPicture;
			$rootScope.currentDisherName = menu.DisherName;
		}

		$scope.isRankMode = function () {
			return $scope.currentMode == classMode.rank;
		};
		$scope.isSearchMode = function () {
			return $scope.currentMode == classMode.search;
		};
		$scope.isResultMode = function () {
			return $scope.currentMode == classMode.result;
		}
		$scope.isOnSaleMode = function () {
			return $scope.currentMode == classMode.onSale;
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
		$scope.enterResultMode = function () {
			_changeMode(classMode.result);
			_filterMenu();
		}
		$scope.enterOnSaleMode = function () {
			_changeMode(classMode.onSale);
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
		$rootScope.addMenu = function (menu, $event) {
			$event.stopPropagation();
			var count = 1;
			if (menu.Additional.Ordered == 0) {
				count = menu.MinCount;
			}
			menu.Additional.Ordered += count;
			rootCart.SizeAll += count;
			rootCart.PriceAll += (menu.DisherPrice * menu.DisherDiscount) * count;
			for (var i = 0; i < $scope.menuSubClass.length; i++) {
				if ($scope.menuSubClass[i].SubClassId == menu.DisherSubclassID1) {
					$scope.menuSubClass[i].Additional.Ordered += count;
					break;
				}
			}
			if (menu.Additional.Ordered == menu.MinCount) {
				rootCart.Results.push(menu);
			}
		};
		$rootScope.removeMenu = function (menu, $event) {
			$event.stopPropagation();
			var count = 1;
			if (menu.Additional.Ordered == menu.MinCount) {
				count = menu.MinCount;
			}
			menu.Additional.Ordered -= count;
			rootCart.SizeAll -= count;
			rootCart.PriceAll -= (menu.DisherPrice * menu.DisherDiscount) * count;
			for (var i = 0; i < $scope.menuSubClass.length; i++) {
				if ($scope.menuSubClass[i].SubClassId == menu.DisherSubclassID1) {
					$scope.menuSubClass[i].Additional.Ordered -= count;
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
		angular.forEach($rootScope.menuDetail, function (menu) {
			menu.Additional.IsNoteCollapsed = false;
		});
		var activeNote = null;
		$scope.toggleNote = function (menu, $event) {
			$event.stopPropagation();
			if (activeNote != null && activeNote != menu) {
				activeNote.Additional.IsNoteCollapsed = false;
			}
			activeNote = menu;
			menu.Additional.IsNoteCollapsed = !menu.Additional.IsNoteCollapsed
		}
		$scope.toggleSetMeal = function (menu) {
			menu.Additional.IsSetMealCollapsed = !menu.Additional.IsSetMealCollapsed;
		}
		// add and remove Remark
		$rootScope.addNote = function (menu, note, index,$event) {
			$event.stopPropagation();
			menu.Additional.Notes.push(note);
			menu.Additional.FilteredNotes.splice(index, 1);
		};
		$rootScope.removeNote = function (menu, note, index, $event) {
			$event.stopPropagation();
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
					filteredArr = $filter('filter')(filteredArr, {
						DisherId: searchText
					});
					break;
				case classMode.result:
					filteredArr = rootCart.Results
					break;
				case classMode.onSale:
					filteredArr = $filter('filter')(filteredArr, {
						IsOnSale: true
					});
					break;
			}

			$scope.filteredMenuDetail = filteredArr;
		}
	}
]).controller('paymentCtrl', [
	'$scope',
	'$rootScope',
	'$location',
	'$http',
	'filterObject',
	'$window',
	function ($scope, $rootScope, $location, $http, filterObject, $window) {
		$http.post('/Waiter/IsAuthenticated').success(function (data) {
			if (!data.IsSucceed) {
				$location.path('/');
			}
		});
		if ($rootScope.cart == null) {
			$location.path('/cart');
			return;
		}

		$http.post('/Waiter/GetTables').success(function (data) {
			$scope.tables = data;
		});

		$rootScope.customersAll = [];
		for (var i = 0; i < 50; i++) {
			$rootScope.customersAll.push(i);
		}
		$http.post('/Cart/GetTablewareFee').success(function (data) {
			$rootScope.tablewareFee = parseFloat(data.TablewareFee);
		});

		$scope.offlinePay = function () {
			for (var i = 0; i < $scope.tables.length; i++) {
				if ($scope.tables[i].DeskId == $scope.deskId) {
					$rootScope.cart.Table = $scope.tables[i];
					break;
				}
			}
			$rootScope.cart.PriceAll = $rootScope.cart.PriceAll + $rootScope.cart.Customer * $rootScope.tablewareFee;

			filterObject($rootScope.cart.Results);
			console.log($rootScope.cart);
			$http.post('/Cart/Submit', $rootScope.cart).success(function (data) {
				delete $rootScope.cart;
				$location.path('/success');
			});
		};

	}
]).controller('successCtrl',function(){})