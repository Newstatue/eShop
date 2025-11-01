namespace Catalog.Data;

public static class Extensions
{
    public static void UseMigration(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProductDbContext>();

        context.Database.Migrate();
        DataSeeder.Seed(context);
    }
}

public static class DataSeeder
{
    public static void Seed(ProductDbContext dbContext)
    {
        if (dbContext.Products.Any())
        {
            return;
        }

        dbContext.Products.AddRange(Products);
        dbContext.SaveChanges();
    }

    public static IEnumerable<Product> Products
    {
        get
        {
            var seedTime = DateTime.UtcNow;

            var outdoorRoot = new ProductCategory
            {
                Name = "户外用品",
                Description = "露营与徒步场景下常用的核心装备"
            };

            var lightingCategory = new ProductCategory
            {
                Name = "照明设备",
                Description = "夜间与环境照明器材",
                ParentCategory = outdoorRoot
            };

            var trekkingCategory = new ProductCategory
            {
                Name = "徒步装备",
                Description = "登山及长距离徒步辅助装备",
                ParentCategory = outdoorRoot
            };

            var apparelCategory = new ProductCategory
            {
                Name = "户外服饰",
                Description = "防护与机能型户外服饰",
                ParentCategory = outdoorRoot
            };

            var survivalCategory = new ProductCategory
            {
                Name = "应急求生",
                Description = "突发状况下的应急工具",
                ParentCategory = outdoorRoot
            };

            var cookingCategory = new ProductCategory
            {
                Name = "营地厨具",
                Description = "营地烹饪与餐饮器具",
                ParentCategory = outdoorRoot
            };

            var shelterCategory = new ProductCategory
            {
                Name = "营地搭建",
                Description = "搭建营地及临时居所的器材",
                ParentCategory = outdoorRoot
            };

            var lightingTag = new ProductTag { Name = "照明" };
            var campingTag = new ProductTag { Name = "露营" };
            var hikingTag = new ProductTag { Name = "徒步" };
            var cookingTag = new ProductTag { Name = "烹饪" };
            var survivalTag = new ProductTag { Name = "求生" };
            var backpackTag = new ProductTag { Name = "背负" };
            var shelterTag = new ProductTag { Name = "帐篷" };
            var gearTag = new ProductTag { Name = "装备" };

            return
            [
                new Product
                {
                    Name = "TrailBlazer 太阳能手电筒",
                    Description = "具备可折叠太阳能板、SOS 警示和 USB 供电的多功能手电。",
                    Brand = "TrailBlazer",
                    RichDescription = "TrailBlazer 太阳能手电筒采用高效太阳能面板，提供强光、泛光及 SOS 模式，可在露营与停电场景下补充照明，也能通过 USB 为设备应急充电，机身抗冲击且便于携带。",
                    IsRichDescriptionAIGenerated = true,
                    Category = lightingCategory,
                    BasePrice = 189.00m,
                    CreatedAt = seedTime.AddDays(-14),
                    Images =
                    [
                        new ProductImage { Url = "solar-flashlight-main.png", IsPrimary = true },
                        new ProductImage { Url = "solar-flashlight-side.png", IsPrimary = false }
                    ],
                    Tags = [lightingTag, survivalTag, campingTag],
                    Variants =
                    [
                        new ProductVariant
                        {
                            Sku = "TB-SOLAR-FLASH-STD",
                            Color = "黑色",
                            Price = 189.00m,
                            StockQuantity = 160,
                            ImageUrl = "solar-flashlight-main.png"
                        },
                        new ProductVariant
                        {
                            Sku = "TB-SOLAR-FLASH-GREEN",
                            Color = "军绿色",
                            Price = 199.00m,
                            StockQuantity = 90,
                            ImageUrl = "solar-flashlight-side.png"
                        }
                    ]
                },
                new Product
                {
                    Name = "SummitPro 碳纤维登山杖",
                    Description = "三节伸缩碳纤维杖身，搭配快拆脚垫与减震系统。",
                    Brand = "SummitPro",
                    RichDescription = "SummitPro 登山杖融合碳纤维和铝合金结构，轻量而稳固。软木手柄吸汗防滑，内置减震机构可缓解膝盖压力，附带四季通用脚垫，适用于多种地形。",
                    IsRichDescriptionAIGenerated = true,
                    Category = trekkingCategory,
                    BasePrice = 129.00m,
                    CreatedAt = seedTime.AddDays(-21),
                    Images =
                    [
                        new ProductImage { Url = "trekking-pole-main.png", IsPrimary = true },
                        new ProductImage { Url = "trekking-pole-accessories.png", IsPrimary = false }
                    ],
                    Tags = [hikingTag, gearTag, campingTag],
                    Variants =
                    [
                        new ProductVariant
                        {
                            Sku = "SP-TREK-110",
                            Color = "曜石黑",
                            Price = 129.00m,
                            StockQuantity = 120,
                            ImageUrl = "trekking-pole-main.png"
                        },
                        new ProductVariant
                        {
                            Sku = "SP-TREK-125",
                            Color = "极地蓝",
                            Price = 149.00m,
                            StockQuantity = 80,
                            ImageUrl = "trekking-pole-accessories.png"
                        }
                    ]
                },
                new Product
                {
                    Name = "SummitWear 风暴三层冲锋衣",
                    Description = "三层防水透气面料，配备帽兜与腋下透气孔。",
                    Brand = "SummitWear",
                    RichDescription = "SummitWear 风暴冲锋衣采用全压胶工艺，兼顾防风、防水与透气，帽兜与袖口均可调节，并设置多功能口袋与腋下拉链，适合城市通勤与高山环境。",
                    IsRichDescriptionAIGenerated = true,
                    Category = apparelCategory,
                    BasePrice = 899.00m,
                    CreatedAt = seedTime.AddDays(-30),
                    Images =
                    [
                        new ProductImage { Url = "outdoor-jacket-main.png", IsPrimary = true },
                        new ProductImage { Url = "outdoor-jacket-back.png", IsPrimary = false }
                    ],
                    Tags = [gearTag, campingTag, hikingTag],
                    Variants =
                    [
                        new ProductVariant
                        {
                            Sku = "SW-JACKET-M",
                            Price = 899.00m,
                            StockQuantity = 70,
                            ImageUrl = "outdoor-jacket-main.png",
                            Size = "M",
                            Color = "深海蓝"
                        },
                        new ProductVariant
                        {
                            Sku = "SW-JACKET-L",
                            Price = 939.00m,
                            StockQuantity = 55,
                            ImageUrl = "outdoor-jacket-back.png",
                            Size = "L",
                            Color = "岩灰绿"
                        }
                    ]
                },
                new Product
                {
                    Name = "RescueReady 全能生存套件",
                    Description = "含多功能刀、打火石、哨子与急救物资的 15 件组合。",
                    Brand = "RescueReady",
                    RichDescription = "RescueReady 生存套件收纳多功能刀、镁棒打火石、求生哨、基础急救包及保暖毯，配备抗压防水硬质收纳盒，是露营、越野与家庭应急的安心选择。",
                    IsRichDescriptionAIGenerated = true,
                    Category = survivalCategory,
                    BasePrice = 459.00m,
                    CreatedAt = seedTime.AddDays(-18),
                    Images =
                    [
                        new ProductImage { Url = "survival-kit-main.png", IsPrimary = true },
                        new ProductImage { Url = "survival-kit-contents.png", IsPrimary = false }
                    ],
                    Tags = [survivalTag, campingTag, gearTag],
                    Variants =
                    [
                        new ProductVariant
                        {
                            Sku = "RR-KIT-STD",
                            Price = 459.00m,
                            StockQuantity = 110,
                            ImageUrl = "survival-kit-main.png"
                        },
                        new ProductVariant
                        {
                            Sku = "RR-KIT-PRO",
                            Price = 519.00m,
                            StockQuantity = 65,
                            ImageUrl = "survival-kit-contents.png"
                        }
                    ]
                },
                new Product
                {
                    Name = "EverPeak 65L 透气背包",
                    Description = "65 升容量，快调背负与防雨罩一体设计。",
                    Brand = "EverPeak",
                    RichDescription = "EverPeak 65L 背包装载轻量骨架与透气背负系统，肩带可快速调节，内部分区合理，底部自带防雨罩，适合多日徒步与长线穿越。",
                    IsRichDescriptionAIGenerated = true,
                    Category = trekkingCategory,
                    BasePrice = 799.00m,
                    CreatedAt = seedTime.AddDays(-27),
                    Images =
                    [
                        new ProductImage { Url = "backpack-main.png", IsPrimary = true },
                        new ProductImage { Url = "backpack-detail.png", IsPrimary = false }
                    ],
                    Tags = [backpackTag, campingTag, hikingTag],
                    Variants =
                    [
                        new ProductVariant
                        {
                            Sku = "EP-PACK-65-BLU",
                            Price = 799.00m,
                            StockQuantity = 90,
                            ImageUrl = "backpack-main.png",
                            Color = "夜空蓝"
                        },
                        new ProductVariant
                        {
                            Sku = "EP-PACK-65-CAM",
                            Price = 849.00m,
                            StockQuantity = 70,
                            ImageUrl = "backpack-detail.png",
                            Color = "森林迷彩"
                        }
                    ]
                },
                new Product
                {
                    Name = "CampChef 双人露营炊具套装",
                    Description = "轻量铝合金材质，包含锅具、碗具与折叠餐具。",
                    Brand = "CampChef",
                    RichDescription = "CampChef 露营炊具套装选用硬氧化铝材，导热均匀易清洁，内含煎锅、炖锅、折叠碗与餐具，并附网袋收纳，满足两人营地烹饪需求。",
                    IsRichDescriptionAIGenerated = true,
                    Category = cookingCategory,
                    BasePrice = 359.00m,
                    CreatedAt = seedTime.AddDays(-12),
                    Images =
                    [
                        new ProductImage { Url = "camp-cookware-main.png", IsPrimary = true },
                        new ProductImage { Url = "camp-cookware-packed.png", IsPrimary = false }
                    ],
                    Tags = [cookingTag, campingTag],
                    Variants =
                    [
                        new ProductVariant
                        {
                            Sku = "CC-COOK-SET-2P",
                            Price = 359.00m,
                            StockQuantity = 95,
                            ImageUrl = "camp-cookware-main.png"
                        },
                        new ProductVariant
                        {
                            Sku = "CC-COOK-SET-4P",
                            Price = 429.00m,
                            StockQuantity = 60,
                            ImageUrl = "camp-cookware-packed.png"
                        }
                    ]
                },
                new Product
                {
                    Name = "FireFly 双燃料折叠炉",
                    Description = "兼容多种燃料罐，双旋钮火力独立调节。",
                    Brand = "FireFly",
                    RichDescription = "FireFly 双燃料折叠炉结构稳固，附带挡风板与点火管，可适配多规格燃料罐。折叠后体积小巧，适用于长途旅行与营地烹饪。",
                    IsRichDescriptionAIGenerated = true,
                    Category = cookingCategory,
                    BasePrice = 499.00m,
                    CreatedAt = seedTime.AddDays(-8),
                    Images =
                    [
                        new ProductImage { Url = "camp-stove-main.png", IsPrimary = true },
                        new ProductImage { Url = "camp-stove-folded.png", IsPrimary = false }
                    ],
                    Tags = [cookingTag, campingTag, gearTag],
                    Variants =
                    [
                        new ProductVariant
                        {
                            Sku = "FF-STOVE-GAS",
                            Price = 499.00m,
                            StockQuantity = 85,
                            ImageUrl = "camp-stove-main.png"
                        },
                        new ProductVariant
                        {
                            Sku = "FF-STOVE-DUAL",
                            Price = 559.00m,
                            StockQuantity = 55,
                            ImageUrl = "camp-stove-folded.png"
                        }
                    ]
                },
                new Product
                {
                    Name = "GlowCamp 双模式营地灯",
                    Description = "暖光与白光可切换，带可拆卸手电与 5200mAh 电源。",
                    Brand = "GlowCamp",
                    RichDescription = "GlowCamp 营地灯提供暖光氛围与冷光照明两种模式，底部隐藏可拆卸手电筒，并内置 5200mAh 电源模块，兼顾夜间照明与设备充电。",
                    IsRichDescriptionAIGenerated = true,
                    Category = lightingCategory,
                    BasePrice = 239.00m,
                    CreatedAt = seedTime.AddDays(-5),
                    Images =
                    [
                        new ProductImage { Url = "camp-lantern-main.png", IsPrimary = true },
                        new ProductImage { Url = "camp-lantern-ambient.png", IsPrimary = false }
                    ],
                    Tags = [lightingTag, campingTag],
                    Variants =
                    [
                        new ProductVariant
                        {
                            Sku = "GC-LANTERN-WARM",
                            Price = 239.00m,
                            StockQuantity = 150,
                            ImageUrl = "camp-lantern-main.png"
                        },
                        new ProductVariant
                        {
                            Sku = "GC-LANTERN-NEON",
                            Price = 259.00m,
                            StockQuantity = 95,
                            ImageUrl = "camp-lantern-ambient.png"
                        }
                    ]
                },
                new Product
                {
                    Name = "SkyShelter 四季双层帐篷",
                    Description = "铝合金骨架配雪裙与全包风绳，适合多季节露营。",
                    Brand = "SkyShelter",
                    RichDescription = "SkyShelter 四季帐篷采用双层结构：外帐防水系数 5000mm，内帐透气防蚊。全包风绳与雪裙设计让帐篷在风雪环境中依旧稳固，是家庭露营与高海拔探险的坚实后盾。",
                    IsRichDescriptionAIGenerated = true,
                    Category = shelterCategory,
                    BasePrice = 1299.00m,
                    CreatedAt = seedTime.AddDays(-25),
                    Images =
                    [
                        new ProductImage { Url = "camp-tent-main.png", IsPrimary = true },
                        new ProductImage { Url = "camp-tent-interior.png", IsPrimary = false }
                    ],
                    Tags = [shelterTag, campingTag, gearTag],
                    Variants =
                    [
                        new ProductVariant
                        {
                            Sku = "SS-TENT-2P",
                            Price = 1299.00m,
                            StockQuantity = 60,
                            ImageUrl = "camp-tent-main.png"
                        },
                        new ProductVariant
                        {
                            Sku = "SS-TENT-4P",
                            Price = 1699.00m,
                            StockQuantity = 35,
                            ImageUrl = "camp-tent-interior.png"
                        }
                    ]
                }
            ];
        }
    }
}
