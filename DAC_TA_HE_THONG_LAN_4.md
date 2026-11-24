1	# Đặc tả hệ thống quản lý bán giày — Nhóm 19, lần 4
2	
3	Nguồn đối chiếu là bản Word `S2_Nhom19_Lan4.docx` tại `C:/Users/lhnhan/source/Shop-giay_v2/`. Phạm vi gồm bối cảnh, yêu cầu, đặc tả use case, sơ đồ tuần tự, mô hình lớp và lược đồ dữ liệu. Tên chức năng được chuẩn hóa chính tả khi cần.
4	
5	## 1. Tổng số hạng mục
6	
7	| Hạng mục | Số lượng | Cách đếm |
8	| --- | ---: | --- |
9	| Chức năng | **24** | Số dòng trong ba bảng yêu cầu mục 3.2 của Chương 2. Khách hàng 13, quản lý cửa hàng 10, quản trị viên kỹ thuật 1. |
10	| Yêu cầu phi chức năng | **6 nhóm** | Theo các đề mục 3.3.1 đến 3.3.6 trong nội dung chính. |
11	| Use case có đặc tả | **23 use case trong 16 nhóm** | Đếm từng bảng có dòng “Tên Use-case” ở Chương 3, mục 1.2. |
12	| Sơ đồ use case | **23 hình tại mục 1.1** | Gồm 1 hình tổng quát và 22 hình chi tiết, tổ chức trong 17 đề mục. Phần đặc tả 1.2 chèn thêm 23 lượt hình minh họa, có hình dùng lại từ mục 1.1. |
13	| Sequence diagram | **42 lượt hình thuộc 16 nhóm** | Có **41 ảnh khác nhau**. Hình hủy đơn của khách được dùng lại trong mục xử lý hủy đơn của quản lý. |
14	| Class diagram | **1 hình tổng quan, 37 lớp** | Một ảnh ở mục 1.4.1. Số lớp lấy từ danh sách mục 1.4.2, gồm thực thể và lớp liên kết. |
15	| RDM | **6 hình, 37 bảng** | Một lược đồ tổng quan, năm lược đồ theo nhóm. Số bảng lấy từ danh sách và từ điển dữ liệu mục 1.5. |
16	
17	Số chức năng, số bản đặc tả và số hình là ba phép đếm khác nhau. Ví dụ, quản lý giỏ hàng là một dòng chức năng, một bản đặc tả use case và ba hình tuần tự.
18	
19	## 2. Bối cảnh và phạm vi
20	
21	Đồ án số hóa hoạt động bán giày trực tuyến. Khách tra cứu sản phẩm theo size và màu, đặt mua, thanh toán và theo dõi đơn. Quản lý cửa hàng vận hành danh mục, sản phẩm, tồn kho, đơn hàng, khuyến mãi và đổi/trả. Quản trị viên kỹ thuật cấp tài khoản, quản lý quyền và theo dõi nhật ký. Các khó khăn chính là giữ tồn kho nhất quán theo biến thể, bảo vệ dữ liệu và xử lý đổi/trả cùng hoàn tiền.
22	
23	Báo cáo sử dụng phân tích hướng đối tượng và UML. Chương 1 mô tả ba cách nghiên cứu gồm khảo sát lý thuyết, khảo sát quy trình bán hàng thực tế và thực nghiệm bằng phiên bản mẫu. Các kịch bản thực nghiệm tập trung vào tìm kiếm, giỏ hàng, đặt đơn, kho và xử lý đơn.
24	
25	## 3. Yêu cầu chức năng
26	
### 3.1 Khách hàng — 13 chức năng
27	
28	| STT | Chức năng | Mô tả |
29	| --- | --- | --- |
30	| 1 | Đăng ký | Tạo tài khoản mới |
31	| 2 | Đăng nhập | Truy cập hệ thống bằng tài khoản đã có |
32	| 3 | Quản lý thông tin tài khoản | Xem và cập nhật thông tin cá nhân |
33	| 4 | Tìm kiếm sản phẩm | Tìm theo tên, mã hoặc từ khóa |
34	| 5 | Lọc sản phẩm | Lọc theo danh mục, thương hiệu, giá, size, màu |
35	| 6 | Xem sản phẩm | Xem thông tin chi tiết và biến thể |
36	| 7 | Quản lý giỏ hàng | Thêm, sửa số lượng, xóa sản phẩm |
37	| 8 | Đặt hàng | Xác nhận thông tin và hoàn tất đơn hàng |
38	| 9 | Thanh toán | Lựa chọn phương thức thanh toán |
39	| 10 | Theo dõi đơn hàng | Theo dõi trạng thái xử lý và giao hàng |
40	| 11 | Xem lịch sử mua hàng | Xem các đơn hàng đã đặt |
41	| 12 | Đánh giá sản phẩm | Đánh giá sau khi đơn hàng hoàn thành |
42	| 13 | Đổi/trả hàng | Gửi yêu cầu đổi hoặc trả sản phẩm |
43	
### 3.2 Quản lý cửa hàng — 10 chức năng
44	
45	| STT | Chức năng | Mô tả |
46	| --- | --- | --- |
47	| 1 | Quản lý sản phẩm | Thêm, sửa, ẩn/ngừng kinh doanh sản phẩm |
48	| 2 | Quản lý danh mục | Thêm, sửa, xóa danh mục |
49	| 3 | Quản lý thương hiệu | Thêm, sửa, xóa thương hiệu |
50	| 4 | Quản lý biến thể | Quản lý size, màu sắc của sản phẩm |
51	| 5 | Quản lý tồn kho | Xem, nhập kho và điều chỉnh tồn kho |
52	| 6 | Quản lý đơn hàng | Xem, xác nhận, cập nhật trạng thái, hủy đơn |
53	| 7 | Quản lý khuyến mãi | Tạo, sửa, kích hoạt và kết thúc khuyến mãi |
54	| 8 | Thống kê báo cáo | Thống kê doanh thu, đơn hàng, sản phẩm bán chạy |
55	| 9 | Quản lý đổi/trả | Tiếp nhận và xử lý yêu cầu đổi/trả |
56	| 10 | Xem thông tin khách hàng | Xem thông tin liên hệ và lịch sử mua hàng |
57	
### 3.3 Quản trị viên kỹ thuật — 1 chức năng
56	
57	| STT | Chức năng | Mô tả |
58	| --- | --- | --- |
59	| 1 | Quản lý tài khoản và phân quyền | Xem danh sách, cấp tài khoản, phân quyền, khóa/mở khóa |
60	
61	Ngoài các dòng ở mục 3.2, phần mô tả và đặc tả còn nêu đăng xuất, khôi phục mật khẩu, hủy đơn, xem chi tiết đơn, nhật ký quản trị và tra cứu khách hàng phục vụ hỗ trợ kỹ thuật. Các luồng này được ghi trong phần tương ứng bên dưới. Không cộng thêm vào tổng 24 dòng của bảng gốc.
62	
63	## 4. Yêu cầu phi chức năng
64	
65	| Nhóm | Nội dung |
66	| --- | --- |
67	| Hiệu năng | Đăng nhập, tìm kiếm, xem sản phẩm và cập nhật giỏ cần phản hồi nhanh. Bản đặc tả không nêu ngưỡng thời gian cụ thể trong phần nội dung này. |
68	| Bảo mật | Bảo vệ tài khoản, thông tin cá nhân và giao dịch. Băm mật khẩu trước khi lưu, kiểm tra quyền theo vai trò, truyền qua kết nối an toàn và ghi nhận thao tác quản trị. |
69	| Dễ sử dụng | Màn hình mua sắm trực quan, thao tác quản lý rõ ràng và trải nghiệm phù hợp trên nhiều thiết bị. |
70	| Chính xác | Giá bán, mức giảm, tổng tiền, biến thể, tồn kho và trạng thái phải đúng. Kiểm tra lại tồn trước khi xác nhận đặt. |
71	| Tin cậy | Thao tác liên quan đơn, thanh toán và kho cần nhất quán. Khi lỗi không được để lại thay đổi dở dang. |
72	| Mở rộng | Có thể bổ sung cổng thanh toán, nhiều chi nhánh, chương trình khách hàng thân thiết và gợi ý sản phẩm. |
73	
74	## 5. Sơ đồ use case và đặc tả
75	
76	Mục 1.1 gồm sơ đồ tổng quát và các sơ đồ cho 16 nhóm nghiệp vụ bên dưới. Hình tổng quát thể hiện ba tác nhân. Một nhóm có thể có nhiều hình chi tiết. Các bảng ở mục 1.2 được ghi lại theo từng use case, gồm tác nhân, điều kiện, luồng chính, ngoại lệ và quy tắc.
77	
77	### 5.1 Danh mục hình use case
78	
79	| Nhóm ở mục 1.2 | Hình chi tiết ở mục 1.1 | Số bản đặc tả |
79	| --- | --- | --- |
80	| Quản lý tài khoản và xác thực | Đăng ký · Đăng nhập · Đăng xuất · Khôi phục mật khẩu | 4 |
81	| Tìm kiếm, lọc và xem chi tiết sản phẩm | Tìm kiếm · Lọc sản phẩm · Xem chi tiết sản phẩm | 3 |
82	| Quản lý giỏ hàng | Quản lý giỏ hàng | 1 |
83	| Đặt hàng | Đặt hàng | 1 |
84	| Thanh toán đơn hàng | Thanh toán đơn hàng | 1 |
85	| Lịch sử và chi tiết đơn hàng | Xem lịch sử mua hàng · Xem chi tiết đơn hàng | 2 |
86	| Hủy đơn hàng | Hủy đơn hàng | 1 |
87	| Quản lý sản phẩm | Quản lý sản phẩm | 1 |
88	| Quản lý biến thể | Quản lý biến thể sản phẩm | 1 |
89	| Quản lý danh mục và thương hiệu | Quản lý danh mục và thương hiệu | 2 |
90	| Quản lý tồn kho | Quản lý tồn kho | 1 |
91	| Quản lý và xử lý đơn hàng | Quản lý và xử lý đơn hàng | 1 |
92	| Xử lý yêu cầu đổi/trả | Xử lý yêu cầu đổi/trả hàng | 1 |
93	| Quản lý khuyến mãi | Quản lý khuyến mãi | 1 |
94	| Thống kê và lập báo cáo kinh doanh | Thống kê và lập báo cáo kinh doanh | 1 |
95	| Quản lý người dùng, phân quyền và kiểm soát hệ thống | Quản lý người dùng, phân quyền và kiểm soát hệ thống | 1 |
96	
97	### 5.2 Nội dung 25 bản đặc tả
98	
99	#### 1.2.1 Quản lý tài khoản và xác thực
100	
101	**1. Đăng ký**
102	
103	**Tác nhân** — Khách hàng
104
105	**Mô tả tóm tắt** — Cho phép khách hàng tạo tài khoản mới trên hệ thống.
106
107	**Mức độ ưu tiên** — Bắt buộc
108
109	**Điều kiện tiên quyết** — Khách hàng truy cập được website.
110
111	**Điều kiện sau** — Thành công: tài khoản được tạo. Thất bại: không ghi nhận tài khoản.
112
113	**Luồng cơ bản**
114
115	1. Khách hàng chọn Đăng ký.
116	2. Nhập thông tin và xác nhận mật khẩu.
117	3. Hệ thống kiểm tra dữ liệu và tính duy nhất.
118	4. Hệ thống tạo tài khoản mới và thông báo thành công.
119
120	**Luồng thay thế và ngoại lệ**
121
122	- Thiếu thông tin hoặc dữ liệu không hợp lệ → yêu cầu sửa.
123	- Email/tên đăng nhập đã tồn tại → từ chối.
124
125	**Quy tắc nghiệp vụ**
126
127	- QT1: Thông tin định danh phải duy nhất.
128	- QT2: Mật khẩu phải được mã hóa trước khi lưu.
129
130	**Yêu cầu phi chức năng**
131
132	- PCN1: Mật khẩu không hiển thị dạng rõ.
133	- PCN2: Truyền dữ liệu qua kết nối bảo mật.
134
135	#### 1.2.2 Đăng nhập
136
137	**2. Đăng nhập**
138
139	**Tác nhân** — Khách hàng
140
141	**Mô tả tóm tắt** — Cho phép khách hàng đăng nhập vào hệ thống.
142
143	**Mức độ ưu tiên** — Bắt buộc
144
145	**Điều kiện tiên quyết** — Tài khoản đã tồn tại.
146
147	**Điều kiện sau** — Thành công: tạo phiên đăng nhập. Thất bại: không tạo phiên.
148
149	**Luồng cơ bản**
150
151	1. Khách hàng nhập tên đăng nhập và mật khẩu.
152	2. Hệ thống xác thực thông tin.
153	3. Hệ thống tạo phiên đăng nhập và chuyển đến trang chủ.
154
155	**Luồng thay thế và ngoại lệ**
156
157	- Sai thông tin đăng nhập → thông báo lỗi.
158	- Tài khoản bị khóa → từ chối đăng nhập.
159
160	**Quy tắc nghiệp vụ** — QT1: Chỉ tài khoản đang hoạt động mới được đăng nhập.
161
162	**Yêu cầu phi chức năng** — PCN1: Thông tin xác thực được bảo mật.
163
164	#### 1.2.3 Đăng xuất
165
166
167
**3. Đăng xuất**
168
169
**Tác nhân** — Khách hàng
170
171
**Mô tả tóm tắt** — Cho phép khách hàng kết thúc phiên đăng nhập hiện tại.
172
173
**Mức độ ưu tiên** — Bắt buộc
174
175
**Điều kiện tiên quyết** — Khách hàng đang có phiên đăng nhập.
176
177
**Điều kiện sau** — Phiên đăng nhập được kết thúc, khách hàng trở về trạng thái chưa đăng nhập.
178
179
**Luồng cơ bản**
180
181
1. Khách hàng chọn Đăng xuất.
182
2. Hệ thống yêu cầu xác nhận.
183
3. Khách hàng xác nhận → Hệ thống kết thúc phiên đăng nhập.
184
185
**Luồng thay thế và ngoại lệ**
186
187
- Khách hàng hủy xác nhận → giữ nguyên phiên đăng nhập.
188
189
**Quy tắc nghiệp vụ** — QT1: Phiên đã đăng xuất không được tiếp tục sử dụng.
190
191
**Yêu cầu phi chức năng** — PCN1: Xóa thông tin xác thực trên giao diện sau khi đăng xuất.
192
193
#### 1.2.4 Khôi phục mật khẩu
194
195
**4. Khôi phục mật khẩu**
196
197
**Tác nhân** — Khách hàng
198
199
**Mô tả tóm tắt** — Cho phép khách hàng yêu cầu và đặt lại mật khẩu khi quên mật khẩu.
200
201
**Mức độ ưu tiên** — Bắt buộc
202
203
**Điều kiện tiên quyết** — Tài khoản đã tồn tại trong hệ thống.
204
205
**Điều kiện sau** — Thành công: mật khẩu được đặt lại. Thất bại: mật khẩu cũ không bị thay đổi.
206
207
**Luồng cơ bản**
208
209
1. Khách hàng chọn Quên mật khẩu và nhập email.
210
2. Hệ thống gửi mã xác minh hoặc liên kết đặt lại.
211
3. Khách hàng nhập mã và mật khẩu mới.
212
4. Hệ thống kiểm tra mã và cập nhật mật khẩu mới.
213
214
**Luồng thay thế và ngoại lệ**
215
216
- Mã xác minh sai, hết hạn hoặc đã sử dụng → từ chối.
217
- Mật khẩu mới không hợp lệ → yêu cầu nhập lại.
218
219
**Quy tắc nghiệp vụ**
220
221
- QT1: Chỉ chủ tài khoản đã xác minh mới được đặt lại mật khẩu.
222
- QT2: Mã xác minh chỉ dùng một lần.
223
224
**Yêu cầu phi chức năng**
225
226
- PCN1: Không tiết lộ email có tồn tại hay không.
227
- PCN2: Mã xác minh có thời hạn.
228
#### 1.2.5 Tìm kiếm sản phẩm
229
230
**5. UC-02A – Tìm kiếm sản phẩm**
231
232
**Tác nhân** — Khách hàng
233
234
**Mô tả tóm tắt** — Cho phép khách hàng tìm kiếm sản phẩm theo từ khóa.
235
236
**Mức độ ưu tiên** — Bắt buộc
237
238
**Điều kiện tiên quyết** — Không bắt buộc đăng nhập.
239
240
**Điều kiện sau** — Hiển thị danh sách sản phẩm phù hợp hoặc danh sách rỗng.
241
242
**Luồng cơ bản**
243
244
1. Khách hàng nhập từ khóa tìm kiếm.
2. Hệ thống trả về danh sách sản phẩm đang công bố phù hợp.
245
246
**Luồng thay thế và ngoại lệ**
247
248
- Không có sản phẩm → hiển thị danh sách rỗng.
249
250
**Quy tắc nghiệp vụ** — QT1: Chỉ hiển thị sản phẩm đang kinh doanh.
251
252
**Yêu cầu phi chức năng** — PCN1: Hỗ trợ tiếng Việt có dấu.
253
#### 1.2.6 Lọc sản phẩm
254
255
**6. UC-02B – Lọc sản phẩm**
256
257
**Tác nhân** — Khách hàng
258
259
**Mô tả tóm tắt** — Cho phép khách hàng lọc danh sách sản phẩm theo tiêu chí.
260
261
**Mức độ ưu tiên** — Bắt buộc
262
263
**Điều kiện tiên quyết** — Không bắt buộc đăng nhập.
264
265
**Điều kiện sau** — Hiển thị danh sách sản phẩm sau khi lọc.
266
267
**Luồng cơ bản**
268
269
1. Khách hàng chọn tiêu chí lọc.
2. Hệ thống áp dụng bộ lọc và cập nhật danh sách.
270
271
**Luồng thay thế và ngoại lệ**
272
273
- Không có sản phẩm phù hợp → hiển thị danh sách rỗng.
274
275
**Quy tắc nghiệp vụ** — QT1: Chỉ lọc trên sản phẩm đang công bố.
276
277
**Yêu cầu phi chức năng** — PCN1: Giao diện sử dụng được trên nhiều thiết bị.
278
#### 1.2.7 Xem chi tiết sản phẩm
279
280
**7. UC-02C – Xem chi tiết sản phẩm**
281
282
**Tác nhân** — Khách hàng
283
284
**Mô tả tóm tóm tắt** — Cho phép khách hàng xem thông tin chi tiết của một sản phẩm trước khi quyết định mua.
285
286
**Mức độ ưu tiên** — Bắt buộc
287
288
**Điều kiện tiên quyết** — Khách hàng truy cập được website. Không bắt buộc đăng nhập.
289
290
**Điều kiện sau** — Thành công: hiển thị đầy đủ thông tin chi tiết sản phẩm.Thất bại: thông báo sản phẩm không khả dụng.
291
292
**Luồng cơ bản**
293
294
1. Khách hàng chọn một sản phẩm từ danh sách hoặc mở liên kết sản phẩm.
295
296
2. Hệ thống lấy thông tin chi tiết sản phẩm và danh sách biến thể.
297
298
3. Hệ thống hiển thị tên, mô tả, giá, hình ảnh, biến thể (size, màu) và tình trạng còn hàng.
299
300
**Luồng thay thế và ngoại lệ**
301
302
- Sản phẩm không tồn tại hoặc không còn công bố → thông báo không khả dụng.
- Biến thể hết hàng → đánh dấu rõ và không cho chọn mua.
303
304
**Quy tắc nghiệp vụ**
305
306
- QT1: Chỉ hiển thị sản phẩm đang được công bố.
- QT2: Giá và tình trạng còn hàng lấy từ dữ liệu hiện hành.
307
308
**Yêu cầu phi chức năng**
309
310
- PCN1: Hỗ trợ tiếng Việt có dấu.
311
- PCN2: Hiển thị tốt trên máy tính và điện thoại.
312
#### 1.2.8 Quản lý sản phẩm
313
314
**14. Quản lý sản phẩm**
315
316
**Tác nhân** — Quản lý cửa hàng
317
318
**Mô tả tóm tắt** — Cho phép quản lý cửa hàng thêm mới, cập nhật thông tin và ẩn/ngừng kinh doanh sản phẩm.
319
320
**Mức độ ưu tiên** — Bắt buộc
321
322
**Điều kiện tiên quyết** — Quản lý đã đăng nhập và có quyền quản lý sản phẩm.
323
324
**Điều kiện sau** — Thành công: sản phẩm được tạo, cập nhật hoặc chuyển sang trạng thái ngừng kinh doanh. Lịch sử giao dịch liên quan đến sản phẩm được bảo toàn.
325
326
**Luồng cơ bản**
327
328
1. Quản lý chọn chức năng Quản lý sản phẩm.
329
2. Hệ thống hiển thị danh sách sản phẩm.
330
3. Thêm sản phẩm: Quản lý nhập thông tin (mã, tên, mô tả, giá, hình ảnh, danh mục, thương hiệu). Hệ thống kiểm tra rồi lưu sản phẩm mới.
331
332
4. Cập nhật sản phẩm: Quản lý chọn sản phẩm, chỉnh sửa thông tin và lưu. Hệ thống cập nhật dữ liệu.
333
334
5. Ẩn/Ngừng kinh doanh: Quản lý chọn sản phẩm và xác nhận. Hệ thống chuyển trạng thái sang “Ngừng kinh doanh”.
335
336
3. Hệ thống thông báo kết quả.
337
338
**Luồng thay thế và ngoại lệ**
339
340
- Mã sản phẩm bị trùng hoặc thiếu thông tin bắt buộc → từ chối lưu và yêu cầu sửa.
341
- Giá bán không hợp lệ (âm hoặc bằng 0) → từ chối.
342
- Sản phẩm đã phát sinh đơn hàng → không cho xóa cứng, chỉ cho phép ẩn/ngừng kinh doanh.
343
- Không tìm thấy sản phẩm → thông báo lỗi.
344
345
**Quy tắc nghiệp vụ**
346
347
- QT1: Mã sản phẩm phải duy nhất.
- QT2: Giá bán không được âm.
- QT3: Không xóa vật lý sản phẩm đã có giao dịch.
348
349
**Yêu cầu phi chức năng**
350
351
- PCN1: Kiểm soát quyền ghi dữ liệu.
- PCN2: Thông báo lỗi rõ ràng từng trường.
352
#### 1.2.9 Quản lý biến thể
353
354
**15. Quản lý biến thể sản phẩm**
355
357
**Tác nhân** — Quản lý cửa hàng
358
359
**Mô tả tóm tóm tắt** — Cho phép quản lý cửa hàng thêm mới, cập nhật thông tin và ẩn biến thể sản phẩm (theo Size và Màu sắc).
360
361
**Mức độ ưu tiên** — Bắt buộc
362
363
**Điều kiện tiên quyết**
364
365
- Quản lý đã đăng nhập và có quyền quản lý sản phẩm.
366
- Sản phẩm gốc đã tồn tại trong hệ thống.
367
368
**Điều kiện sau** — Thành công: biến thể được tạo mới, cập nhật thông tin hoặc chuyển sang trạng thái ngừng kinh doanh.
369
370
**Luồng cơ bản**
371
372
1. Quản lý chọn sản phẩm cần quản lý biến thể.
373
374
2. Hệ thống hiển thị danh sách biến thể hiện có của sản phẩm đó.
375
3. Thêm biến thể: Quản lý nhập Size, Màu sắc. Hệ thống kiểm tra trùng rồi tạo biến thể mới.
376
377
4. Cập nhật biến thể: Quản lý chọn biến thể, chỉnh sửa Size hoặc Màu sắc (nếu được phép) và lưu.
378
379
5. Hệ thống thông báo kết quả thao tác.
380
381
**Luồng thay thế và ngoại lệ**
382
383
- Tổ hợp Size + Màu sắc đã tồn tại trong cùng sản phẩm → từ chối thêm và thông báo trùng.
- Không tìm thấy biến thể cần sửa/ẩn → thông báo lỗi.
- Biến thể đã phát sinh trong đơn hàng → không cho xóa cứng, chỉ cho phép ẩn.
384
385
**Quy tắc nghiệp vụ**
386
387
- QT1: Tổ hợp Size + Màu sắc phải là duy nhất trong cùng một sản phẩm.
- QT2: Không xóa vật lý biến thể đã từng xuất hiện trong đơn hàng.
- QT3: Việc thay đổi số lượng tồn kho của biến thể thuộc về chức năng Quản lý tồn kho (UC-08).
388
389
**Yêu cầu phi chức năng**
390
391
- PCN1: Kiểm soát quyền khi thêm/sửa/ẩn biến thể.
- PCN2: Thông báo lỗi rõ ràng khi bị trùng Size + Màu.
392
#### 1.2.10 Quản lý danh mục và thương hiệu
393
394
**16. Quản lý danh mục**
395
396
**Tác nhân** — Quản lý cửa hàng
397
398
**Mô tả tóm tóm tắt** — Cho phép quản lý cửa hàng thêm mới, cập nhật và ẩn danh mục sản phẩm.
399
400
**Mức độ ưu tiên** — Bắt buộc
401
402
**Điều kiện tiên quyết** — Quản lý đã đăng nhập và có quyền quản lý danh mục.
403
404
**Điều kiện sau** — Thành công: danh mục được tạo, cập nhật hoặc chuyển sang trạng thái ngừng sử dụng.
405
406
**Luồng cơ bản**
407
408
1. Quản lý chọn chức năng Quản lý danh mục.
409
2. Hệ thống hiển thị danh sách danh mục hiện có.
410
3. Thêm danh mục: Quản lý nhập thông tin và lưu.
411
4. Cập nhật danh mục: Quản lý chọn danh mục, chỉnh sửa và lưu.
412
5. Ẩn danh mục: Quản lý xác nhận. Hệ thống chuyển trạng thái ngừng sử dụng.
413
414
**Luồng thay thế và ngoại lệ**
415
416
- Tên danh mục bị trùng → từ chối.
- Danh mục đã có sản phẩm liên kết → không cho xóa cứng, chỉ cho phép ẩn.
417
418
**Quy tắc nghiệp vụ**
419
420
- QT1: Không xóa cứng danh mục đã được sử dụng.
- QT2: Thay đổi danh mục không làm sai lệch dữ liệu đơn hàng cũ.
421
422
**Yêu cầu phi chức năng**
423
424
- PCN1: Kiểm soát quyền.
- PCN2: Thông báo rõ ràng khi không thể xóa.
425
426
**17. Quản lý thương hiệu**
427
428
**Tác nhân** — Quản lý cửa hàng
429
430
**Mô tả tóm tóm tóm tắt** — Cho phép quản lý cửa hàng thêm mới, cập nhật và ẩn thương hiệu sản phẩm.
431
432
**Mức độ ưu tiên** — Bắt buộc
433
434
**Điều kiện tiên quyết** — Quản lý đã đăng nhập và có quyền quản lý thương hiệu.
435
436
**Điều kiện sau** — Thành công: thương hiệu được tạo, cập nhật hoặc chuyển sang trạng thái ngừng sử dụng.
437
438
**Luồng cơ bản**
439
440
1. Quản lý chọn chức năng Quản lý thương hiệu.
441
2. Hệ thống hiển thị danh sách thương hiệu hiện có.
442
3. Thêm thương hiệu: Quản lý nhập thông tin và lưu.
443
4. Cập nhật thương hiệu: Quản lý chọn thương hiệu, chỉnh sửa và lưu.
444
4. Ẩn thương hiệu: Quản lý xác nhận. Hệ thống chuyển trạng thái ngừng sử dụng.
445
446
**Luồng thay thế và ngoại lệ**
447
448
- Tên thương hiệu bị trùng → từ chối.
- Thương hiệu đã có sản phẩm liên kết → không cho xóa cứng, chỉ cho phép ẩn.
449
450
**Quy tắc nghiệp vụ**
451
452
- QT1: Không xóa cứng thương hiệu đã được sử dụng.
- QT2: Thay đổi thương hiệu không làm sai lệch dữ liệu đơn hàng cũ.
453
454
**Yêu cầu phi chức năng**
455
456
- PCN1: Kiểm soát quyền.
- PCN2: Thông báo rõ ràng khi không thể xóa.
457
#### 1.2.11 Quản lý tồn kho
458
459
**18. Quản lý tồn kho**
460
461
**Tác nhân** — Quản lý cửa hàng
462
463
**Mô tả tóm tóm tóm tắt** — Cho phép quản lý cửa hàng xem tồn kho theo biến thể, nhập kho và điều chỉnh tồn kho.
464
465
**Mức độ ưu tiên** — Bắt buộc
466
467
**Trạng thái đặc tả** — Đã hoàn thiện theo sơ đồ Use-case cuối cùng
468
469
**Điều kiện tiên quyết**
470
471
- Quản lý đã đăng nhập và có quyền quản lý kho.
472
- Sản phẩm và biến thể cần ghi nhận tồn kho đã tồn tại.
473
474
**Điều kiện sau**
474
475
- Thành công: tồn kho biến thể được hiển thị hoặc số lượng được cập nhật kèm chứng từ, lý do và người thực hiện.
475
476
- Không thành công: không phát sinh biến động kho dở dang.
476
477
**Luồng cơ bản**
477
478
1. Quản lý chọn Xem tồn kho biến thể.
479
480
2. Hệ thống hiển thị số lượng tồn và phần đang được phân bổ theo từng biến thể.
481
482
3. Khi nhận hàng, quản lý chọn Nhập kho và nhập biến thể, số lượng cùng thông tin tham chiếu.
483
484
4. Hệ thống kiểm tra dữ liệu, ghi nhận lần nhập và tăng tồn kho tương ứng.
485
486
5. Khi có chênh lệch kiểm kê, quản lý chọn Điều chỉnh tồn kho, nhập số lượng kiểm đếm và lý do.
487
488
6. Hệ thống kiểm tra dữ liệu, ghi nhận chênh lệch và cập nhật tồn kho.
489
490
7. Hệ thống hiển thị số lượng sau thao tác và lưu lịch sử biến động.
491
492
**Luồng thay thế và ngoại lệ**
493
494
- 2a. Chưa có dữ liệu tồn: hiển thị số lượng bằng không theo biến thể.
- 3a. Số lượng nhập không phải số nguyên dương hoặc biến thể không hợp lệ: từ chối ghi nhận.
- 5a. Thiếu lý do điều chỉnh: yêu cầu bổ sung.
- 6a. Điều chỉnh làm tồn khả dụng âm hoặc mâu thuẫn với hàng đã phân bổ: từ chối và yêu cầu xử lý chênh lệch.
- 4a, 6b. Dữ liệu bị thay đổi đồng thời: tải lại số lượng trước khi xác nhận.
495
496
**Quy tắc nghiệp vụ**
497
498
- QT1: Không có chức năng Xuất kho độc lập; biến động do bán hàng được xử lý trong quy trình đơn hàng.
- QT2: Mọi lần nhập và điều chỉnh phải có người thực hiện và thời điểm.
- QT3: Không cho phép tồn kho âm.
- QT4: Không được nhập lại cùng chứng từ để tăng kho hai lần.
499
500
**Yêu cầu phi chức năng**
501
502
- PCN1: Cập nhật số lượng và lịch sử phải nhất quán.
- PCN2: Chỉ người có quyền mới được nhập hoặc điều chỉnh.
- PCN3: Dữ liệu tồn phải có thể đối chiếu với lịch sử biến động.
503
#### 1.2.12 Quản lý và xử lý đơn hàng
504
505
**19. Quản lý và xử lý đơn hàng**
506
507
**Tác nhân** — Quản lý cửa hàng
508
509
**Mô tả tóm tóm tóm tóm tóm** — Cho phép quản lý cửa hàng xem danh sách và chi tiết đơn hàng, xác nhận đơn, cập nhật trạng thái và hủy đơn hàng.
510
511
**Mức độ ưu tiên** — Bắt buộc
512
513
**Điều kiện tiên quyết** — Quản lý đã đăng nhập và có quyền xử lý đơn hàng.
514
515
**Điều kiện sau** — Thành công: đơn hàng được xác nhận, cập nhật trạng thái hoặc hủy theo đúng quy trình. Tồn kho và lịch sử được cập nhật nhất quán.
516
517
**Luồng cơ bản**
518
519
1. Quản lý chọn chức năng Quản lý đơn hàng.
520
521
2. Hệ thống hiển thị danh sách đơn hàng.
522
3. Quản lý chọn một đơn hàng để xem chi tiết.
523
4. Xác nhận đơn: Quản lý xác nhận đơn đang ở trạng thái “Chờ xác nhận”. Hệ thống chuyển sang “Đã xác nhận”.
524
5. Cập nhật trạng thái: Quản lý chọn trạng thái tiếp theo phù hợp (Đang chuẩn bị → Đang giao → Đã giao → Hoàn thành). Hệ thống cập nhật và ghi lịch sử.
525
6. Hủy đơn: Quản lý chọn hủy, nhập lý do và xác nhận. Hệ thống chuyển sang “Đã hủy”, hoàn trả tồn kho nếu cần.
526
7. Hệ thống thông báo kết quả.
527
528
**Luồng thay thế và ngoại lệ**
529
530
- Không có đơn hàng → hiển thị danh sách trống.
- Đơn hàng không còn ở trạng thái cho phép xác nhận/hủy → từ chối.
- Trạng thái mới không hợp lệ theo quy trình → từ chối cập nhật.
- Dữ liệu bị thay đổi đồng thời → yêu cầu tải lại.
- Lưu thất bại → thông báo lỗi, không thay đổi trạng thái và tồn kho.
531
532
**Quy tắc nghiệp vụ**
533
534
- QT1: Chỉ cho phép chuyển trạng thái theo đúng quy trình nghiệp vụ.
- QT2: Hủy đơn chỉ được thực hiện khi đơn còn ở trạng thái cho phép.
- QT3: Khi hủy phải hoàn trả đúng số lượng tồn kho đã phân bổ.
- QT4: COD không bắt buộc phải thanh toán trước khi xác nhận.
535
536
**Yêu cầu phi chức năng**
537
538
- PCN1: Ghi nhận người xử lý và thời điểm.
- PCN2: Đảm bảo nhất quán giữa trạng thái đơn và tồn kho.
- PCN3: Bảo vệ thông tin khách hàng.
539
#### 1.2.13 Xử lý yêu cầu đổi/trả
540
541
**20. Xử lý yêu cầu đổi/trả hàng**
542
543
**Tác nhân** — Quản lý cửa hàng
544
545
**Mô tả tóm tóm tóm tóm** — Cho phép quản lý cửa hàng tiếp nhận, phê duyệt hoặc từ chối yêu cầu đổi/trả hàng; thực hiện hoàn tiền và nhận hàng hoàn về khi yêu cầu được chấp nhận.
546
547
**Mức độ ưu tiên** — Bắt buộc
548
549
**Điều kiện tiên quyết** — Quản lý đã đăng nhập và có quyền xử lý đổi/trả. Đã có yêu cầu đổi/trả từ khách hàng gắn với đơn hàng hợp lệ.
550
551
**Điều kiện sau** — Thành công: yêu cầu được phê duyệt hoặc từ chối. Nếu được duyệt thì thực hiện hoàn tiền và/hoặc cập nhật tồn kho khi nhận hàng hoàn về.
552
553
**Luồng cơ bản**
554
555
1. Quản lý chọn chức năng Quản lý yêu cầu đổi/trả.
556
557
2. Hệ thống hiển thị danh sách yêu cầu đang chờ xử lý.
558
558
3. Quản lý chọn một yêu cầu để xem chi tiết (đơn hàng, sản phẩm, lý do, hình ảnh đính kèm nếu có).
559
560
4. Quản lý đưa ra quyết định: Phê duyệt hoặc Từ chối và nhập lý do.
561
562

5. Nếu từ chối: Hệ thống lưu quyết định và thông báo cho khách hàng. Kết thúc xử lý.
563
564

6. Nếu phê duyệt:   - Hệ thống xác định các bước cần thực hiện (hoàn tiền và/hoặc nhận hàng hoàn về).   - Hoàn tiền: Quản lý xác nhận số tiền hoàn. Hệ thống ghi nhận hoàn tiền khi có bằng chứng hợp lệ.   - Nhận hàng hoàn về: Quản lý xác nhận đã nhận hàng, kiểm tra tình trạng. Hệ thống cập nhật lại tồn kho nếu hàng đủ điều kiện bán lại.
565

7. Hệ thống lưu toàn bộ kết quả xử lý và thông báo trạng thái cuối cùng.
566
567
**Luồng thay thế và ngoại lệ**
568
569
- Không tìm thấy yêu cầu hoặc đơn hàng liên quan → thông báo lỗi.
- Số lượng yêu cầu đổi/trả vượt quá số lượng đã mua → từ chối.
- Hoàn tiền thất bại hoặc đang chờ xác nhận → giữ trạng thái chưa hoàn tất.
- Hàng hoàn về không đủ điều kiện bán lại → không tăng tồn kho khả dụng.
- Yêu cầu đã được xử lý trước đó → không cho xử lý trùng.
570
571
**Quy tắc nghiệp vụ**
572
573
- QT1: Chỉ xử lý hoàn tiền và nhận hàng sau khi yêu cầu được phê duyệt.
- QT2: Số tiền hoàn không vượt quá số tiền khách đã thanh toán cho phần hàng được chấp nhận.
- QT3: Chỉ cập nhật tồn kho khi hàng hoàn về đạt điều kiện.
- QT4: Mỗi yêu cầu chỉ được xử lý một lần.
574
575
**Yêu cầu phi chức năng**
576
577
- PCN1: Ghi nhận đầy đủ người xử lý, thời điểm, quyết định và bằng chứng.
- PCN2: Đảm bảo không ghi nhận trùng hoàn tiền hoặc nhập kho.
- PCN3: Chỉ người có quyền mới được phê duyệt và xác nhận hoàn tiền/hàng về.
578
#### 1.2.14 Quản lý khuyến mãi
579
580
**21. Quản lý khuyến mãi**
581
582
**Tác nhân** — Quản lý cửa hàng
583
584
**Mô tả tóm tóm tóm** — Cho phép quản lý tạo, sửa, kích hoạt và kết thúc các chương trình khuyến mãi.
585
586
**Mức độ ưu tiên** — Bắt buộc
587
588
**Trạng thái đặc tả** — Đã hoàn thiện theo sơ đồ Use-case cuối cùng
589
590
**Điều kiện tiên quyết**
591
592
- Quản lý đã đăng nhập và có quyền quản lý khuyến mãi.
593
- Chương trình cần sửa, kích hoạt hoặc kết thúc đã tồn tại.
594
595
**Điều kiện sau**
596
597
- Thành công: chương trình được lưu với nội dung và trạng thái tương ứng.
- Các đơn đã lập giữ nguyên mức giảm giá được ghi nhận tại thời điểm tạo đơn.
598
599
**Luồng cơ bản**
600
601
1. Quản lý chọn Tạo khuyến mãi, nhập tên, hình thức giảm, giá trị, thời gian và phạm vi áp dụng.
602
603
2. Hệ thống kiểm tra dữ liệu và lưu chương trình ở trạng thái chưa kích hoạt.
604
3. Khi chọn Sửa khuyến mãi, quản lý thay đổi thông tin được phép; hệ thống kiểm tra và lưu.
605
4. Khi chọn Kích hoạt khuyến mãi, hệ thống kiểm tra thời hạn và điều kiện; quản lý xác nhận đưa chương trình vào áp dụng.
606
5. Khi chọn Kết thúc khuyến mãi, quản lý xác nhận dừng; hệ thống ngừng áp dụng cho giao dịch mới.
607
6. Hệ thống thông báo kết quả và lưu lịch sử thay đổi.
608
609
**Luồng thay thế và ngoại lệ**
610
611
- 2a, 3a. Thiếu thông tin, thời gian kết thúc trước thời gian bắt đầu hoặc giá trị giảm không hợp lệ: yêu cầu sửa.
- 4a. Chương trình hết hạn hoặc không còn đáp ứng điều kiện: từ chối kích hoạt.
- 3b. Thay đổi ảnh hưởng đơn đã tạo: chỉ áp dụng dữ liệu mới cho giao dịch sau khi thay đổi có hiệu lực.
- 5a. Chương trình đã kết thúc: trả lại trạng thái hiện tại, không xử lý lặp.
612
613
**Quy tắc nghiệp vụ**
614
615
- QT1: Thời gian hiệu lực và đối tượng áp dụng phải xác định rõ.
- QT2: Mức giảm không được làm giá trị thanh toán của phần hàng áp dụng nhỏ hơn không.
- QT3: Nếu nhiều chương trình cùng hợp lệ, áp dụng theo quy tắc kết hợp được cấu hình.
- QT4: Chương trình kết thúc không được áp dụng cho đơn mới.
616
617
**Yêu cầu phi chức năng**
618
619
- PCN1: Kiểm soát quyền và lưu lịch sử thay đổi.
- PCN2: Tính giảm giá nhất quán giữa giỏ hàng và lúc xác nhận đơn.
- PCN3: Thông báo rõ trạng thái và thời gian hiệu lực.
620
#### 1.2.15 Thống kê và lập báo cáo kinh doanh
621
622
**22. Thống kê và lập báo cáo kinh doanh**
623
624
**Tác nhân** — Quản lý cửa hàng
625
626
**Mô tả tóm tóm tóm** — Cho phép quản lý cửa hàng xem thống kê doanh thu, số lượng đơn hàng và lập báo cáo kinh doanh theo khoảng thời gian lựa chọn.
627
628
**Mức độ ưu tiên** — Nên có
629
630
**Điều kiện tiên quyết**
631
632
- Quản lý đã đăng nhập và có quyền xem báo cáo.
633
- Hệ thống có thể truy cập dữ liệu đơn hàng, thanh toán và hoàn tiền.
634
635
**Điều kiện sau**
636
637
- Thành công: Hệ thống hiển thị số liệu thống kê hoặc báo cáo theo đúng tiêu chí đã chọn. Không có dữ liệu: Hiển thị giá trị bằng 0 hoặc danh sách rỗng, không tạo số liệu giả.
638
639
**Luồng cơ bản**
640
641
- 1. Quản lý chọn chức năng Thống kê và báo cáo.
642
643
- 2. Hệ thống hiển thị giao diện cho phép chọn khoảng thời gian và các tiêu chí lọc (nếu có).
644
- 3. Quản lý nhập khoảng thời gian cần xem báo cáo.
645
- 4. Quản lý chọn loại thống kê cần xem:   - Tổng hợp doanh thu   - Thống kê số lượng đơn hàng theo trạng thái   - Báo cáo tổng hợp kinh doanh
645
- 5. Hệ thống tổng hợp dữ liệu theo tiêu chí đã chọn (chỉ lấy các khoản thu và hoàn tiền đã được xác nhận).
646
- 6. Hệ thống hiển thị kết quả kèm theo khoảng thời gian, tiêu chí và thời điểm tổng hợp.
647
- 7. Nếu quản lý chọn xuất báo cáo, hệ thống tạo báo cáo để xem hoặc tải về.
648
649
**Luồng thay thế và ngoại lệ**
650
651
- Khoảng thời gian không hợp lệ (ngày kết thúc trước ngày bắt đầu) → yêu cầu nhập lại.
652
- Không có giao dịch trong khoảng thời gian → hiển thị số liệu bằng 0 và thông báo không có dữ liệu.
- Có đơn hàng đang chờ xác nhận hoặc COD chưa thu → hệ thống trình bày riêng, không cộng vào doanh thu đã xác nhận.
- Không lấy được đủ dữ liệu → thông báo lỗi và không hiển thị báo cáo không hoàn chỉnh.
653
654
**Quy tắc nghiệp vụ**
655
656
- QT1: Doanh thu = Tổng khoản thu đã xác nhận – Tổng khoản hoàn đã xác nhận trong kỳ báo cáo.
- QT2: Số đơn hàng được đếm theo mã đơn duy nhất theo ngày tạo; đơn hủy được thống kê riêng.
- QT3: Không nhân đôi giá trị khi một đơn có nhiều lần thanh toán hoặc nhiều sản phẩm.
- QT4: Chỉ các giao dịch đã được xác nhận mới được đưa vào số liệu chính thức.
657
658
**Yêu cầu phi chức năng**
659
660
- PCN1: Chỉ người có quyền mới được xem dữ liệu báo cáo.
- PCN2: Kết quả báo cáo phải có thể đối chiếu lại với dữ liệu gốc.
- PCN3: Báo cáo phải thể hiện rõ đơn vị tiền tệ, khoảng thời gian và thời điểm tạo báo cáo.
- PCN4: Khi đang tổng hợp dữ liệu, giao diện cần có trạng thái xử lý.
661
#### 1.2.16 Quản lý người dùng, phân quyền và kiểm soát hệ thống
662
663
**23. Quản lý người dùng, phân quyền và kiểm soát hệ thống**
664
665
**Tác nhân** — Quản trị hệ thống (System Admin)
666
667
**Mô tả tóm tóm tóm** — Cho phép quản trị hệ thống thêm mới, sửa thông tin, vô hiệu hóa/mở khóa tài khoản, phân quyền vai trò và theo dõi nhật ký các thao tác quản trị.
668
669
**Mức độ ưu tiên** — Bắt buộc
670
671
**Điều kiện tiên quyết** — Quản trị hệ thống đã đăng nhập bằng tài khoản đang hoạt động và có đầy đủ quyền quản trị.
672
673
**Điều kiện sau** — Thành công: tài khoản được tạo, cập nhật, vô hiệu hóa hoặc được gán/thu hồi quyền; mọi thao tác đều được ghi vào nhật ký.Thất bại: dữ liệu không bị thay đổi trái phép.
674
675
**Luồng cơ bản**
676
677
1. Quản trị chọn chức năng Quản lý người dùng.
678
679
2. Hệ thống hiển thị danh sách tài khoản hiện có.
680
3. Thêm tài khoản: Quản trị nhập thông tin và chọn vai trò (Khách hàng / Shop Manager / System Admin). Hệ thống kiểm tra tính duy nhất rồi tạo tài khoản.
681
4. Sửa thông tin tài khoản: Quản trị chọn tài khoản, chỉnh sửa thông tin được phép và lưu.
682
5. Vô hiệu hóa / Mở khóa tài khoản: Quản trị chọn tài khoản và xác nhận. Hệ thống cập nhật trạng thái hoạt động.
683
6. Phân quyền: Quản trị chọn tài khoản, gán hoặc thu hồi vai trò và xác nhận.
679
680
7. Hệ thống ghi nhận toàn bộ thao tác vào nhật ký (người thực hiện, thời điểm, nội dung, kết quả) và thông báo kết quả.
681
682
**Luồng thay thế và ngoại lệ**
683
684
- Email hoặc số điện thoại đã tồn tại → từ chối tạo tài khoản.
685
- Không tìm thấy tài khoản → thông báo lỗi.
686
- Cố gắng vô hiệu hóa tài khoản quản trị đang hoạt động cuối cùng → từ chối.
- Cố gắng tự nâng quyền của chính mình vượt quá phạm vi → từ chối.
687
- Thao tác thất bại → không thay đổi dữ liệu và vẫn ghi nhận vào nhật ký.
688
**Quy tắc nghiệp vụ**
689
690
- QT1: Không xóa vật lý tài khoản đã phát sinh dữ liệu, chỉ được vô hiệu hóa.
- QT2: Không được vô hiệu hóa tài khoản quản trị đang hoạt động cuối cùng.
- QT3: Không cho phép người dùng tự nâng quyền của chính mình.
- QT4: Mọi thao tác quản trị quan trọng phải được ghi nhận vào nhật ký.
- QT5: Admin có quyền tạo và quản lý tài khoản Shop Manager.
691
692
**Yêu cầu phi chức năng**
693
694
- PCN1: Kiểm tra quyền ở phía hệ thống cho mọi yêu cầu.
- PCN2: Không lưu mật khẩu dạng rõ trong nhật ký.
- PCN3: Quyền bị thu hồi phải có hiệu lực ngay ở lần kiểm tra tiếp theo.
695
#### 1.2.17 Đánh giá sản phẩm
696
697
**24. Đánh giá sản phẩm**
698
699
**Tác nhân** — Khách hàng
697
698

**Mô tả tóm tóm** — Cho phép khách hàng đánh giá sản phẩm sau khi nhận đơn hàng.
699

**Mức độ ưu tiên** — Bắt buộc
700

**Điều kiện tiên quyết** — Khách hàng đã nhận đơn hàng và đã đánh giá chưa.
701

**Điều kiện sau** — Thành công: đánh giá được lưu và hiển thị. Thất bại: đánh giá không được lưu.
702

**Luồng cơ bản**
703

1. Khách hàng chọn sản phẩm cần đánh giá từ lịch sử đơn hàng.
704
2. Khách hàng nhập số sao (1-5) và nội dung nhận xét.
705
3. Hệ thống lưu đánh giá và liên kết với đơn hàng và khách hàng.
706
4. Hệ thống hiển thị kết quả và cập nhật điểm trung bình.
707

**Quy tắc nghiệp vụ**
708

- QT1: Mỗi khách chỉ được đánh giá một lần mỗi đơn hàng.
- QT2: Điểm đánh giá từ 1 đến 5 sao.
- QT3: Nội dung đánh giá không được để trống.
709

**Yêu cầu phi chức năng**
710

- PCN1: Thông tin đánh giá phải được bảo vệ.
- PCN2: Điểm trung bình phải tính đúng từ các đánh giá thực tế.
711
#### 1.2.18 Quản lý thông tin khách hàng
697
698

**25. Quản lý thông tin khách hàng**
699
700

**Tác nhân** — Khách hàng / Quản trị hệ thống
701
702

**Mô tả tóm tóm** — Cho phép khách hàng sửa thông tin hồ sơ cá nhân và quản trị hệ thống xem/chỉnh sửa hồ sơ.
703

**Mức độ ưu tiên** — Nên có
704

**Điều kiện tiên quyết** — Khách hàng đã đăng nhập.
705

**Điều kiện sau** — Thành công: thông tin khách hàng được cập nhật. Thất bại: cập nhật thất bại do dữ liệu không hợp lệ.
706

**Luồng cơ bản**
707

1. Khách hàng chọn sửa thông tin hồ sơ.
708
2. Khách hàng thay đổi thông tin (họ tên, email, số điện thoại, địa chỉ).
709
3. Hệ thống kiểm tra dữ liệu và lưu thay đổi.
710
4. Hệ thống hiển thị kết quả và thông báo.
711

**Quy tắc nghiệp vụ**
712

- QT1: Khách hàng chỉ được sửa hồ sơ của chính mình.
- QT2: Email và số điện thoại phải duy nhất trong hệ thống.
- QT3: Thay đổi mật khẩu cần xác minh lại qua email hoặc SMS.
713

**Yêu cầu phi chức năng**
714

- PCN1: Thông tin cá nhân phải được bảo vệ và mã hóa.
- PCN2: Không lưu mật khẩu dạng rõ trong nhật ký.
- PCN3: Các thay đổi phải có lịch sử ghi nhận.
715

## 6. Danh mục sơ đồ tuần tự
716

| Mục Chương 3 | Nhóm | Các sơ đồ trong Word | Lượt hình |
| --- | --- | --- | --- |
| 1.3.1 | Quản lý tài khoản và xác thực | Đăng ký · Đăng nhập · Đăng xuất · Khôi phục mật khẩu | 4 |
| 1.3.2 | Tìm kiếm, lọc và xem chi tiết sản phẩm | Tìm kiếm · Lọc sản phẩm · Xem chi tiết sản phẩm | 3 |
| 1.3.3 | Quản lý giỏ hàng | Thêm vào giỏ · Cập nhật giỏ hàng · Xóa sản phẩm khỏi giỏ | 3 |
| 1.3.4 | Đặt hàng | Đặt hàng | 1 |
| 1.3.5 | Thanh toán đơn hàng | Thanh toán đơn hàng | 1 |
| 1.3.6 | Lịch sử và chi tiết đơn hàng | Xem lịch sử mua hàng · Xem chi tiết đơn hàng | 2 |
| 1.3.7 | Hủy đơn hàng | Khách hủy đơn hàng | 1 |
| 1.3.8 | Quản lý sản phẩm | Thêm sản phẩm · Cập nhật sản phẩm · Ẩn hoặc ngừng kinh doanh | 3 |
| 1.3.9 | Quản lý biến thể | Xem danh sách · Thêm biến thể · Cập nhật biến thể · Ẩn biến thể | 4 |
| 1.3.10 | Quản lý danh mục và thương hiệu | Quản lý danh mục · Quản lý thương hiệu | 2 |
| 1.3.11 | Quản lý tồn kho | Xem tồn kho · Nhập kho · Điều chỉnh tồn kho | 3 |
| 1.3.12 | Quản lý và xử lý đơn hàng | Xem danh sách đơn · Xác nhận đơn · Xử lý hủy đơn (dùng lại ảnh khách hủy đơn) | 3 |
| 1.3.13 | Xử lý yêu cầu đổi/trả | Phê duyệt hoặc từ chối yêu cầu · Xử lý hàng hoàn và hoàn tiền | 2 |
| 1.3.14 | Quản lý khuyến mãi | Tạo chương trình · Kích hoạt · Kết thúc | 3 |
| 1.3.15 | Thống kê và lập báo cáo kinh doanh | Tổng hợp doanh thu · Thống kê số lượng đơn · Lập báo cáo tổng hợp | 3 |
| 1.3.16 | Quản lý người dùng, phân quyền và kiểm soát hệ thống | Thêm tài khoản · Vô hiệu hóa hoặc mở khóa · Sửa thông tin · Phân quyền | 4 |

Luồng sửa chương trình khuyến mãi có trong use case nhưng mục 1.3.14 chỉ có hình tạo, kích hoạt và kết thúc. Mục 1.3.12 dùng lại ảnh “Hủy đơn hàng” có tác nhân khách hàng. Danh mục trên đếm ảnh trong Word.

## 7. Sơ đồ lớp và mô hình quan hệ

### 7.1 Class diagram

Hình 3.1.71 là sơ đồ lớp tổng quan. Mục 1.4.2 liệt kê 37 lớp, tương ứng 37 thực thể hoặc bảng liên kết. Các tên lớp và trách nhiệm được đối chiếu theo bảng dưới đây.

| STT | Lớp / bảng | Vai trò |
| --- | --- | --- |
| 1 | `VaiTro` | Lưu các vai trò được sử dụng để phân quyền cho người dùng. |
| 2 | `TaiKhoan` | Lưu thông tin đăng nhập, hồ sơ cá nhân và trạng thái bảo mật của tài khoản. |
| 3 | `TaiKhoanVaiTro` | Bảng liên kết tài khoản với vai trò, đồng thời ghi nhận người và thời điểm gán. |
| 4 | `PhienDangNhap` | Quản lý phiên xác thực, thời hạn token và thời điểm thu hồi phiên. |
| 5 | `DatLaiMatKhau` | Quản lý yêu cầu khôi phục mật khẩu bằng token giới hạn thời gian và số lần thử. |
| 6 | `NhatKyHeThong` | Lưu nhật ký thao tác, kết quả thực hiện và thông tin truy vết của hệ thống. |
| 7 | `DanhMuc` | Lưu danh sách danh mục dùng để phân loại sản phẩm giày. |
| 8 | `ThuongHieu` | Lưu danh sách thương hiệu của các sản phẩm giày. |
| 9 | `SanPham` | Lưu thông tin sản phẩm, giá bán cơ sở, danh mục, thương hiệu và trạng thái. |
| 10 | `KichCo` | Lưu các cỡ giày theo hệ kích cỡ và thứ tự hiển thị. |
| 11 | `MauSac` | Lưu danh sách màu sắc và mã màu của sản phẩm. |
| 12 | `BienTheSanPham` | Lưu từng biến thể của sản phẩm theo tổ hợp kích cỡ và màu sắc, kèm SKU. |
| 13 | `HinhAnhSanPham` | Lưu hình ảnh chung của sản phẩm hoặc hình ảnh riêng của biến thể. |
| 14 | `TonKho` | Lưu tồn kho vật lý, số lượng đã phân bổ và tồn khả dụng cho từng biến thể. |
| 15 | `PhieuKho` | Lưu chứng từ nhập kho, điều chỉnh kho hoặc nhận hàng trả. |
| 16 | `ChiTietPhieuKho` | Lưu từng biến thể và số lượng biến động thuộc một phiếu kho. |
| 17 | `PhanBoTonKho` | Lưu số lượng tồn kho giữ cho từng dòng hàng trong đơn và trạng thái phân bổ. |
| 18 | `BienDongKho` | Ghi sổ mọi thay đổi tồn vật lý và tồn đã phân bổ để đối soát. |
| 19 | `GioHang` | Lưu giỏ hàng của tài khoản hoặc giỏ hàng khách chưa đăng nhập theo phiên. |
| 20 | `ChiTietGioHang` | Lưu các biến thể giày và số lượng tương ứng trong giỏ hàng. |
| 21 | `PhuongThucThanhToan` | Lưu các phương thức thanh toán áp dụng cho đơn hàng. |
| 22 | `DonHang` | Lưu đơn đặt hàng, địa chỉ nhận, trạng thái và tổng số tiền cần thanh toán. |
| 23 | `ChiTietDonHang` | Lưu các dòng hàng và thông tin sản phẩm được chụp tại thời điểm mua. |
| 24 | `LichSuDonHang` | Ghi lại mỗi lần chuyển trạng thái của đơn hàng. |
| 25 | `ThanhToan` | Lưu từng giao dịch thanh toán, trạng thái xác nhận và chứng từ đối soát. |
| 26 | `KhuyenMai` | Lưu chương trình giảm giá, điều kiện áp dụng, phạm vi và thời hạn hiệu lực. |
| 27 | `KhuyenMaiSanPham` | Bảng liên kết khuyến mãi với các sản phẩm thuộc phạm vi áp dụng. |
| 28 | `KhuyenMaiDanhMuc` | Bảng liên kết khuyến mãi với các danh mục thuộc phạm vi áp dụng. |
| 29 | `KhuyenMaiThuongHieu` | Bảng liên kết khuyến mãi với các thương hiệu thuộc phạm vi áp dụng. |
| 30 | `KhuyenMaiDonHang` | Lưu chương trình đã áp dụng trên đơn hàng và số tiền được giảm thực tế. |
| 31 | `LichSuKhuyenMai` | Ghi lịch sử tạo, sửa, kích hoạt và kết thúc chương trình khuyến mãi. |
| 32 | `YeuCauDoiTra` | Lưu yêu cầu đổi hoặc trả hàng, trạng thái duyệt và đơn đổi mới nếu có. |
| 33 | `ChiTietDoiTra` | Lưu các dòng hàng đổi/trả, số lượng xử lý và khoản hoàn được duyệt. |
| 34 | `HinhAnhDoiTra` | Lưu hình ảnh làm bằng chứng cho yêu cầu đổi/trả. |
| 35 | `LichSuDoiTra` | Ghi nhận lịch sử xử lý và thay đổi trạng thái yêu cầu đổi/trả. |
| 36 | `HoanTien` | Lưu giao dịch hoàn tiền gắn với khoản thanh toán gốc và yêu cầu đổi/trả (nếu có). |
| 37 | `DanhGia` | Lưu đánh giá và điểm số của khách hàng cho từng dòng hàng đã mua. |

#### Thuộc tính và kiểu dữ liệu của lớp

Mục 1.4.3 mô tả thuộc tính ở cấp lớp. Kiểu dữ liệu SQL tương thích có trong từ điển dữ liệu ở mục 7.3.

##### VaiTro

Lưu các vai trò được sử dụng để phân quyền cho người dùng.

| STT | Thuộc tính và kiểu | Mô tả |
| --- | --- | --- |
| 1 | `VaiTroID: int` | Mã định danh vai trò (PK) |
| 2 | `MaVaiTro: string` | Mã vai trò duy nhất (CUSTOMER, SHOP_MANAGER, SYSTEM_ADMIN) (UK) |
| 3 | `TenVaiTro: string` | Tên hiển thị của vai trò |
| 4 | `MoTa: string` | Thông tin mô tả quyền hạn hoặc trách nhiệm của vai trò |

##### TaiKhoan

Lưu thông tin đăng nhập, hồ sơ cá nhân và trạng thái bảo mật của tài khoản.

| STT | Thuộc tính và kiểu | Mô tả |
| --- | --- | --- |
| 1 | `TaiKhoanID: long` | Mã định danh tài khoản (PK) |
| 2 | `TenDangNhap: string` | Tên đăng nhập duy nhất (UK) |
| 3 | `Email: string` | Địa chỉ email duy nhất của tài khoản (UK) |
| 4 | `SoDienThoai: string` | Số điện thoại liên hệ, nếu cung cấp thì không trùng |
| 5 | `MatKhauHash: string` | Giá trị băm của mật khẩu, không lưu mật khẩu dạng rõ |
| 6 | `HoTen: string` | Họ và tên chủ tài khoản |
| 7 | `NgaySinh: DateTime` | Ngày sinh của người dùng |
| 8 | `GioiTinh: string` | Giới tính do người dùng khai báo |
| 9 | `DiaChi: string` | Địa chỉ liên hệ trong hồ sơ cá nhân |
| 10 | `AnhDaiDien: string` | Đường dẫn hoặc URL ảnh đại diện |
| 11 | `TrangThai: string` | Trạng thái tài khoản: HOAT_DONG, BI_KHOA hoặc VO_HIEU_HOA |
| 12 | `EmailDaXacMinh: bool` | Cờ xác nhận địa chỉ email |
| 13 | `SoLanDangNhapSai: int` | Số lần đăng nhập sai đã ghi nhận |
| 14 | `KhoaDenLuc: DateTime` | Thời điểm kết thúc khóa tạm thời |
| 15 | `LanDangNhapCuoi: DateTime` | Thời điểm đăng nhập gần nhất |
| 16 | `NgayDoiMatKhau: DateTime` | Thời điểm thay đổi mật khẩu gần nhất |
| 17 | `NgayTao: DateTime` | Thời điểm tạo tài khoản (UTC) |
| 18 | `NgayCapNhat: DateTime` | Thời điểm cập nhật hồ sơ (UTC) |
| 19 | `PhienBan: byte[]` | Giá trị rowversion để kiểm soát cập nhật đồng thời |

##### TaiKhoanVaiTro

Bảng liên kết tài khoản với vai trò, đồng thời ghi nhận người và thời điểm gán.

| STT | Thuộc tính và kiểu | Mô tả |
| --- | --- | --- |
| 1 | `TaiKhoanID: long` | Tài khoản được gán vai trò (PK, FK) |
| 2 | `VaiTroID: int` | Vai trò được gán cho tài khoản (PK, FK) |
| 3 | `NguoiGanID: long` | Tài khoản thực hiện gán vai trò (FK) |
| 4 | `NgayGan: DateTime` | Thời điểm gán vai trò (UTC) |

##### PhienDangNhap

Quản lý phiên xác thực, thời hạn token và thời điểm thu hồi phiên.

| STT | Thuộc tính và kiểu | Mô tả |
| --- | --- | --- |
| 1 | `PhienID: Guid` | Mã định danh duy nhất của phiên đăng nhập (PK) |
| 2 | `TaiKhoanID: long` | Tài khoản sở hữu phiên (FK) |
| 3 | `TokenHash: byte[]` | Giá trị băm của token xác thực, không lưu token gốc (UK) |
| 4 | `NgayTao: DateTime` | Thời điểm tạo phiên (UTC) |
| 5 | `HetHanLuc: DateTime` | Thời điểm hết hạn phiên |
| 6 | `ThuHoiLuc: DateTime` | Thời điểm phiên bị thu hồi hoặc đăng xuất |
| 7 | `DiaChiIP: string` | Địa chỉ IP của thiết bị đăng nhập |
| 8 | `UserAgent: string` | Chuỗi nhận dạng trình duyệt hoặc thiết bị |

##### DatLaiMatKhau

Quản lý yêu cầu khôi phục mật khẩu bằng token giới hạn thời gian và số lần thử.

| STT | Thuộc tính và kiểu | Mô tả |
| --- | --- | --- |
| 1 | `YeuCauID: long` | Mã yêu cầu đặt lại mật khẩu (PK) |
| 2 | `TaiKhoanID: long` | Tài khoản yêu cầu khôi phục mật khẩu (FK) |
| 3 | `TokenHash: byte[]` | Giá trị băm token đặt lại mật khẩu, duy nhất (UK) |
| 4 | `NgayTao: DateTime` | Thời điểm phát sinh yêu cầu |
| 5 | `HetHanLuc: DateTime` | Thời điểm hết hiệu lực token |
| 6 | `DaSuDungLuc: DateTime` | Thời điểm token đã được sử dụng |
| 7 | `SoLanThu: int` | Số lượt thử sử dụng token |

##### NhatKyHeThong

Lưu nhật ký thao tác, kết quả thực hiện và thông tin truy vết của hệ thống.

| STT | Thuộc tính và kiểu | Mô tả |
| --- | --- | --- |
| 1 | `NhatKyID: long` | Mã nhật ký hệ thống (PK) |
| 2 | `NguoiThucHienID: long` | Tài khoản thực hiện thao tác, có thể trống nếu chưa đăng nhập (FK) |
| 3 | `HanhDong: string` | Tên hành động được ghi nhận |
| 4 | `DoiTuong: string` | Loại đối tượng bị tác động |
| 5 | `MaDoiTuong: string` | Mã đối tượng cụ thể bị tác động |
| 6 | `NoiDung: string` | Thông tin diễn giải hành động, không chứa mật khẩu/token |
| 7 | `KetQua: string` | Kết quả: THANH_CONG hoặc THAT_BAI |
| 8 | `DiaChiIP: string` | Địa chỉ IP phát sinh thao tác |
| 9 | `MaYeuCau: Guid` | Mã truy vết yêu cầu xử lý |
| 10 | `ThoiDiem: DateTime` | Thời điểm ghi nhật ký |

##### DanhMuc

Lưu danh sách danh mục dùng để phân loại sản phẩm giày.

| STT | Thuộc tính và kiểu | Mô tả |
| --- | --- | --- |
| 1 | `DanhMucID: int` | Mã danh mục sản phẩm (PK) |
| 2 | `TenDanhMuc: string` | Tên danh mục duy nhất (UK) |
| 3 | `MoTa: string` | Mô tả danh mục |
| 4 | `HinhAnh: string` | Đường dẫn ảnh đại diện danh mục |
| 5 | `DangHoatDong: bool` | Cờ danh mục còn được sử dụng |
| 6 | `NgayTao: DateTime` | Ngày giờ tạo danh mục |
| 7 | `NgayCapNhat: DateTime` | Ngày giờ cập nhật danh mục |

##### ThuongHieu

Lưu danh sách thương hiệu của các sản phẩm giày.

| STT | Thuộc tính và kiểu | Mô tả |
| --- | --- | --- |
| 1 | `ThuongHieuID: int` | Mã thương hiệu (PK) |
| 2 | `TenThuongHieu: string` | Tên thương hiệu duy nhất (UK) |
| 3 | `MoTa: string` | Mô tả thương hiệu |
| 4 | `HinhAnh: string` | Đường dẫn ảnh thương hiệu |
| 5 | `DangHoatDong: bool` | Cờ trạng thái hoạt động của thương hiệu |
| 6 | `NgayTao: DateTime` | Ngày giờ tạo thương hiệu |
| 7 | `NgayCapNhat: DateTime` | Ngày giờ cập nhật thương hiệu |

##### SanPham

Lưu thông tin sản phẩm, giá bán cơ sở, danh mục, thương hiệu và trạng thái.

| STT | Thuộc tính và kiểu | Mô tả |
| --- | --- | --- |
| 1 | `SanPhamID: long` | Mã định danh sản phẩm (PK) |
| 2 | `MaSanPham: string` | Mã nghiệp vụ của sản phẩm, không trùng (UK) |
| 3 | `TenSanPham: string` | Tên sản phẩm giày |
| 4 | `MoTa: string` | Mô tả chi tiết sản phẩm |
| 5 | `GiaBan: decimal` | Giá bán cơ sở của sản phẩm, lớn hơn 0 |
| 6 | `DanhMucID: int` | Danh mục phân loại sản phẩm (FK) |
| 7 | `ThuongHieuID: int` | Thương hiệu của sản phẩm (FK) |
| 8 | `TrangThai: string` | Trạng thái hiển thị/kinh doanh: NHAP, DANG_BAN, AN, NGUNG_BAN |
| 9 | `NguoiTaoID: long` | Tài khoản đã tạo sản phẩm (FK) |
| 10 | `NgayTao: DateTime` | Thời điểm tạo sản phẩm |
| 11 | `NgayCapNhat: DateTime` | Thời điểm cập nhật sản phẩm |
| 12 | `PhienBan: byte[]` | Rowversion hỗ trợ kiểm soát đồng thời |

##### KichCo

Lưu các cỡ giày theo hệ kích cỡ và thứ tự hiển thị.

| STT | Thuộc tính và kiểu | Mô tả |
| --- | --- | --- |
| 1 | `KichCoID: int` | Mã định danh kích cỡ (PK) |
| 2 | `HeSize: string` | Hệ cỡ giày như EU, US hoặc UK |
| 3 | `GiaTri: string` | Giá trị kích cỡ trong hệ đã chọn |
| 4 | `ThuTu: int` | Thứ tự hiển thị kích cỡ |
| 5 | `DangHoatDong: bool` | Cờ cho biết kích cỡ đang dùng |

##### MauSac

Lưu danh sách màu sắc và mã màu của sản phẩm.

| STT | Thuộc tính và kiểu | Mô tả |
| --- | --- | --- |
| 1 | `MauSacID: int` | Mã định danh màu sắc (PK) |
| 2 | `TenMau: string` | Tên màu sắc duy nhất (UK) |
| 3 | `MaHex: string` | Mã màu dạng #RRGGBB khi có |
| 4 | `DangHoatDong: bool` | Cờ sử dụng màu sắc |

##### BienTheSanPham

Lưu từng biến thể của sản phẩm theo tổ hợp kích cỡ và màu sắc, kèm SKU.

| STT | Thuộc tính và kiểu | Mô tả |
| --- | --- | --- |
| 1 | `BienTheID: long` | Mã định danh biến thể (PK) |
| 2 | `SanPhamID: long` | Sản phẩm cha của biến thể (FK) |
| 3 | `SKU: string` | Mã quản lý tồn kho duy nhất của biến thể (UK) |
| 4 | `KichCoID: int` | Kích cỡ của biến thể (FK) |
| 5 | `MauSacID: int` | Màu sắc của biến thể (FK) |
| 6 | `GiaBanRieng: decimal` | Giá riêng nếu biến thể không dùng giá sản phẩm |
| 7 | `DangKinhDoanh: bool` | Cờ biến thể còn được phép bán |
| 8 | `NgayTao: DateTime` | Thời điểm tạo biến thể |
| 9 | `NgayCapNhat: DateTime` | Thời điểm cập nhật biến thể |
| 10 | `PhienBan: byte[]` | Rowversion cho kiểm soát ghi đồng thời |

##### HinhAnhSanPham

Lưu hình ảnh chung của sản phẩm hoặc hình ảnh riêng của biến thể.

| STT | Thuộc tính và kiểu | Mô tả |
| --- | --- | --- |
| 1 | `HinhAnhID: long` | Mã hình ảnh sản phẩm (PK) |
| 2 | `SanPhamID: long` | Sản phẩm mà hình ảnh thuộc về (FK) |
| 3 | `BienTheID: long` | Biến thể được ảnh minh họa, có thể để trống nếu là ảnh chung (FK) |
| 4 | `DuongDan: string` | Đường dẫn hoặc URL đến tệp hình ảnh |
| 5 | `MoTaAnh: string` | Nội dung mô tả hình ảnh |
| 6 | `LaAnhChinh: bool` | Cờ đánh dấu ảnh chính; mỗi sản phẩm tối đa một ảnh chính |
| 7 | `ThuTu: int` | Thứ tự sắp xếp hình ảnh |

##### TonKho

Lưu tồn kho vật lý, số lượng đã phân bổ và tồn khả dụng cho từng biến thể.

| STT | Thuộc tính và kiểu | Mô tả |
| --- | --- | --- |
| 1 | `BienTheID: long` | Biến thể giày tương ứng với bản ghi tồn kho (PK, FK) |
| 2 | `SoLuongTon: int` | Số lượng hàng thực tế đang có trong kho |
| 3 | `SoLuongDaPhanBo: int` | Số lượng đã giữ/phân bổ cho các đơn hàng |
| 4 | `SoLuongKhaDung: int (tính toán)` | Số lượng có thể bán; được tính bằng SoLuongTon - SoLuongDaPhanBo |
| 5 | `MucCanhBao: int` | Ngưỡng số lượng tồn thấp để cảnh báo |
| 6 | `NgayCapNhat: DateTime` | Thời điểm cập nhật dữ liệu tồn kho |
| 7 | `PhienBan: byte[]` | Rowversion để tránh xung đột khi cập nhật số lượng |

##### PhieuKho

Lưu chứng từ nhập kho, điều chỉnh kho hoặc nhận hàng trả.

| STT | Thuộc tính và kiểu | Mô tả |
| --- | --- | --- |
| 1 | `PhieuKhoID: long` | Mã định danh phiếu kho (PK) |
| 2 | `MaPhieu: string` | Mã nghiệp vụ của phiếu kho, không trùng (UK) |
| 3 | `MaChongLap: Guid` | Mã chống tạo trùng phiếu kho (UK) |
| 4 | `LoaiPhieu: string` | Loại phiếu: NHAP, DIEU_CHINH hoặc NHAN_TRA |
| 5 | `MaChungTuThamChieu: string` | Mã chứng từ liên quan dùng để đối soát |
| 6 | `LyDo: string` | Diễn giải lý do lập phiếu kho |
| 7 | `NguoiThucHienID: long` | Tài khoản thực hiện nghiệp vụ kho (FK) |
| 8 | `NgayTao: DateTime` | Thời điểm tạo phiếu |

##### ChiTietPhieuKho

Lưu từng biến thể và số lượng biến động thuộc một phiếu kho.

| STT | Thuộc tính và kiểu | Mô tả |
| --- | --- | --- |
| 1 | `ChiTietPhieuKhoID: long` | Mã định danh dòng phiếu kho (PK) |
| 2 | `PhieuKhoID: long` | Phiếu kho chứa dòng phát sinh (FK) |
| 3 | `BienTheID: long` | Biến thể được nhập/điều chỉnh/nhân trả (FK) |
| 4 | `SoLuongThayDoi: int` | Số lượng thay đổi (+/-), không được bằng 0 |
| 5 | `DonGiaNhap: decimal` | Đơn giá nhập hàng nếu áp dụng |
| 6 | `ChiTietDoiTraID: long` | Dòng đổi/trả liên quan khi nhận hàng trả về (FK) |
| 7 | `GhiChu: string` | Ghi chú xử lý đối với dòng phiếu kho |

##### PhanBoTonKho

Lưu số lượng tồn kho giữ cho từng dòng hàng trong đơn và trạng thái phân bổ.

| STT | Thuộc tính và kiểu | Mô tả |
| --- | --- | --- |
| 1 | `ChiTietDonHangID: long` | Dòng đơn hàng được giữ tồn kho (PK, FK) |
| 2 | `SoLuongGiu: int` | Số lượng giữ riêng cho dòng đơn hàng |
| 3 | `TrangThai: string` | Trạng thái giữ hàng: DANG_GIU, DA_XUAT, DA_GIAI_PHONG |
| 4 | `NgayPhanBo: DateTime` | Thời điểm tạo phân bổ kho |
| 5 | `HanGiuLuc: DateTime` | Hạn cuối giữ hàng, nếu có |
| 6 | `NgayKetThuc: DateTime` | Thời điểm kết thúc giữ hàng hoặc xuất kho |
| 7 | `PhienBan: byte[]` | Rowversion tránh trùng lặp cập nhật phân bổ |

##### BienDongKho

Ghi sổ mọi thay đổi tồn vật lý và tồn đã phân bổ để đối soát.

| STT | Thuộc tính và kiểu | Mô tả |
| --- | --- | --- |
| 1 | `BienDongID: long` | Mã bản ghi biến động kho (PK) |
| 2 | `BienTheID: long` | Biến thể có số lượng thay đổi (FK) |
| 3 | `LoaiBienDong: string` | Loại biến động: NHAP, DIEU_CHINH, GIU_HANG, GIAI_PHONG, BAN_HANG, NHAN_TRA |
| 4 | `ThayDoiTon: int` | Mức thay đổi của số lượng tồn vật lý |
| 5 | `ThayDoiPhanBo: int` | Mức thay đổi của số lượng đang phân bổ |
| 6 | `TonSau: int` | Số lượng tồn thực tế sau nghiệp vụ |
| 7 | `PhanBoSau: int` | Số lượng đã phân bổ sau nghiệp vụ |
| 8 | `ChiTietPhieuKhoID: long` | Dòng phiếu kho làm phát sinh biến động (nếu có) (FK) |
| 9 | `ChiTietDonHangID: long` | Dòng đơn hàng làm phát sinh biến động (nếu có) (FK) |
| 10 | `NguoiThucHienID: long` | Tài khoản thực hiện nghiệp vụ (FK) |
| 11 | `MaThaoTac: Guid` | Mã duy nhất của thao tác chống ghi sổ trùng (UK) |
| 12 | `LyDo: string` | Nội dung diễn giải biến động |
| 13 | `ThoiDiem: DateTime` | Thời điểm ghi sổ kho |

##### GioHang

Lưu giỏ hàng của tài khoản hoặc giỏ hàng khách chưa đăng nhập theo phiên.

| STT | Thuộc tính và kiểu | Mô tả |
| --- | --- | --- |
| 1 | `GioHangID: long` | Mã giỏ hàng (PK) |
| 2 | `TaiKhoanID: long` | Chủ giỏ hàng khi người dùng đã đăng nhập (FK) |
| 3 | `MaPhienHash: byte[]` | Mã băm phiên của khách chưa đăng nhập |
| 4 | `NgayTao: DateTime` | Thời điểm tạo giỏ hàng |
| 5 | `NgayCapNhat: DateTime` | Thời điểm cập nhật giỏ hàng |
| 6 | `HetHanLuc: DateTime` | Thời điểm giỏ của khách hết hạn (nếu áp dụng) |

##### ChiTietGioHang

Lưu các biến thể giày và số lượng tương ứng trong giỏ hàng.

| STT | Thuộc tính và kiểu | Mô tả |
| --- | --- | --- |
| 1 | `GioHangID: long` | Giỏ hàng chứa mặt hàng (PK, FK) |
| 2 | `BienTheID: long` | Biến thể giày được chọn mua (PK, FK) |
| 3 | `SoLuong: int` | Số lượng sản phẩm của biến thể trong giỏ, phải lớn hơn 0 |
| 4 | `NgayThem: DateTime` | Thời điểm thêm biến thể vào giỏ |
| 5 | `NgayCapNhat: DateTime` | Thời điểm cập nhật số lượng trong giỏ |

##### PhuongThucThanhToan

Lưu các phương thức thanh toán áp dụng cho đơn hàng.

| STT | Thuộc tính và kiểu | Mô tả |
| --- | --- | --- |
| 1 | `PhuongThucID: int` | Mã phương thức thanh toán (PK) |
| 2 | `MaPhuongThuc` | `varchar(30)` | Mã phương thức duy nhất (COD, BANK_TRANSFER, ONLINE) (UK) |
| 3 | `TenPhuongThuc` | `nvarchar(100)` | Tên phương thức dùng để hiển thị |
| 4 | `DangHoatDong: bit` | Cờ phương thức đang được hỗ trợ |

##### DonHang

Lưu đơn đặt hàng, địa chỉ nhận, trạng thái và tổng số tiền cần thanh toán.

| STT | Thuộc tính và kiểu | Mô tả |
| --- | --- | --- |
| 1 | `DonHangID: long` | Mã định danh đơn hàng (PK) |
| 2 | `MaDonHang: string` | Mã hiển thị đơn hàng duy nhất (UK) |
| 3 | `MaYeuCauTao: uniqueidentifier` | Mã chống tạo đơn hàng trùng khi gửi yêu cầu nhiều lần (UK) |
| 4 | `KhachHangID: long` | Tài khoản khách hàng sở hữu đơn hàng (FK) |
| 5 | `HoTenNguoiNhan: string` | Họ tên người nhận được lưu tại thời điểm đặt |
| 6 | `SoDienThoaiNhan: string` | Số điện thoại người nhận |
| 7 | `EmailNhan: string` | Email liên hệ người nhận, nếu có |
| 8 | `DiaChiNhan: string` | Địa chỉ nhận hàng |
| 9 | `PhuongXa: string` | Phường/xã của địa chỉ giao hàng |
| 10 | `TinhThanh: string` | Tỉnh/thành phố giao hàng |
| 11 | `GhiChu: string` | Ghi chú giao hàng từ khách |
| 12 | `PhuongThucID: int` | Phương thức thanh toán khách đã chọn (FK) |
| 13 | `TrangThai: string` | Trạng thái xử lý đơn từ chờ xác nhận đến hoàn thành hoặc hủy |
| 14 | `TienHang: decimal(18,2)` | Tổng giá trị các dòng hàng trước giảm giá |
| 15 | `TienGiam: decimal(18,2)` | Tổng số tiền khuyến mãi được trừ |
| 16 | `PhiVanChuyen: decimal(18,2)` | Phí vận chuyển của đơn hàng |
| 17 | `TongThanhToan: decimal(18,2) (tính toán)` | Tổng phải thanh toán, tính bằng TienHang - TienGiam + PhiVanChuyen |
| 18 | `TienTe: char(3)` | Đơn vị tiền tệ; SQL cố định giá trị VND |
| 19 | `NgayDat: datetime2(0)` | Thời điểm khách đặt hàng |
| 20 | `HanThanhToan: datetime2(0)` | Hạn thanh toán của đơn hàng (nếu có) |
| 21 | `NgayGiao: datetime2(0)` | Thời điểm đơn được ghi nhận giao hàng |
| 22 | `NgayHoanThanh: datetime2(0)` | Thời điểm đơn hoàn thành |
| 23 | `NgayHuy: datetime2(0)` | Thời điểm hủy đơn, bắt buộc khi trạng thái DA_HUY |
| 24 | `LyDoHuy: string` | Lý do đơn hàng bị hủy |
| 25 | `NguoiHuyID: long` | Tài khoản thực hiện thao tác hủy đơn (FK) |
| 26 | `PhienBan: byte[]` | Rowversion kiểm soát cập nhật đơn |

##### ChiTietDonHang

Lưu các dòng hàng và thông tin sản phẩm được chụp tại thời điểm mua.

| STT | Thuộc tính và kiểu | Mô tả |
| --- | --- | --- |
| 1 | `ChiTietDonHangID: long` | Mã định danh dòng đơn hàng (PK) |
| 2 | `DonHangID: long` | Đơn hàng chứa dòng sản phẩm (FK) |
| 3 | `BienTheID: long` | Biến thể giày đã mua (FK) |
| 4 | `MaSanPhamLucMua: string` | Mã sản phẩm được chụp tại thời điểm mua |
| 5 | `SKULucMua: string` | SKU biến thể tại thời điểm mua |
| 6 | `TenSanPhamLucMua: string` | Tên sản phẩm tại thời điểm mua |
| 7 | `KichCoLucMua: string` | Kích cỡ sản phẩm tại thời điểm mua |
| 8 | `MauSacLucMua: string` | Màu sắc sản phẩm tại thời điểm mua |
| 9 | `DanhMucLucMua: string` | Tên danh mục tại thời điểm mua |
| 10 | `ThuongHieuLucMua: string` | Tên thương hiệu tại thời điểm mua |
| 11 | `SoLuong: int` | Số lượng biến thể được đặt mua |
| 12 | `DonGia: decimal(18,2)` | Đơn giá mỗi sản phẩm tại thời điểm mua |
| 13 | `TienGiamPhanBo: decimal(18,2)` | Phần tiền khuyến mãi được phân bổ vào dòng hàng |
| 14 | `ThanhTien: decimal(18,2) (tính toán)` | Số tiền dòng hàng sau giảm giá; tính SoLuong × DonGia - TienGiamPhanBo |

##### LichSuDonHang

Ghi lại mỗi lần chuyển trạng thái của đơn hàng.

| STT | Thuộc tính và kiểu | Mô tả |
| --- | --- | --- |
| 1 | `LichSuID: long` | Mã bản ghi lịch sử trạng thái (PK) |
| 2 | `DonHangID: long` | Đơn hàng có thay đổi (FK) |
| 3 | `TrangThaiCu: string` | Trạng thái trước khi thay đổi |
| 4 | `TrangThaiMoi: string` | Trạng thái sau khi thay đổi |
| 5 | `NguoiThucHienID: long` | Tài khoản thực hiện chuyển trạng thái (FK) |
| 6 | `ThoiDiem: datetime2(0)` | Thời điểm phát sinh thay đổi |
| 7 | `GhiChu: string` | Ghi chú hoặc lý do cho lần chuyển trạng thái |

##### ThanhToan

Lưu từng giao dịch thanh toán, trạng thái xác nhận và chứng từ đối soát.

| STT | Thuộc tính và kiểu | Mô tả |
| --- | --- | --- |
| 1 | `ThanhToanID: long` | Mã định danh giao dịch thanh toán (PK) |
| 2 | `DonHangID: long` | Đơn hàng được thanh toán (FK) |
| 3 | `PhuongThucID: int` | Phương thức sử dụng trong giao dịch (FK) |
| 4 | `MaYeuCau: uniqueidentifier` | Mã yêu cầu chống xử lý lại một giao dịch (UK) |
| 5 | `CongThanhToan: string` | Tên cổng hoặc kênh thanh toán được sử dụng |
| 6 | `MaGiaoDich: string` | Mã giao dịch do cổng thanh toán cung cấp |
| 7 | `SoTien: decimal(18,2)` | Số tiền của giao dịch thanh toán, phải lớn hơn 0 |
| 8 | `TrangThai: string` | Trạng thái xác nhận, thất bại, hủy hoặc hết hạn |
| 9 | `NgayTao: datetime2(0)` | Thời điểm tạo giao dịch thanh toán |
| 10 | `NgayXacNhan: datetime2(0)` | Thời điểm giao dịch đã được xác nhận thu tiền |
| 11 | `NguoiXacNhanID: long` | Tài khoản xác nhận giao dịch (nếu có) (FK) |
| 12 | `BangChung: string` | Đường dẫn/tài liệu chứng minh thanh toán |
| 13 | `ThongBaoLoi: string` | Thông báo lỗi khi giao dịch không thành công |
| 14 | `PhienBan: byte[]` | Rowversion kiểm soát xác nhận giao dịch đồng thời |

##### KhuyenMai

Lưu chương trình giảm giá, điều kiện áp dụng, phạm vi và thời hạn hiệu lực.

| STT | Thuộc tính và kiểu | Mô tả |
| --- | --- | --- |
| 1 | `KhuyenMaiID: long` | Mã định danh chương trình khuyến mãi (PK) |
| 2 | `MaKhuyenMai: string` | Mã chương trình duy nhất (UK) |
| 3 | `TenKhuyenMai: string` | Tên khuyến mãi |
| 4 | `MoTa: string` | Mô tả nội dung, điều khoản của chương trình |
| 5 | `HinhThuc: string` | Hình thức giảm giá: PHAN_TRAM hoặc SO_TIEN |
| 6 | `GiaTri: decimal(18,2)` | Mức giảm theo phần trăm hoặc số tiền |
| 7 | `GiamToiDa: decimal(18,2)` | Giới hạn số tiền giảm tối đa, nếu thiết lập |
| 8 | `GiaTriDonToiThieu: decimal(18,2)` | Giá trị đơn tối thiểu để được áp dụng |
| 9 | `PhamVi: string` | Phạm vi: TOAN_BO, SAN_PHAM, DANH_MUC, THUONG_HIEU |
| 10 | `BatDauLuc: datetime2(0)` | Thời điểm bắt đầu hiệu lực |
| 11 | `KetThucLuc: datetime2(0)` | Thời điểm kết thúc hiệu lực |
| 12 | `TrangThai: string` | Trạng thái NHAP, DANG_AP_DUNG hoặc KET_THUC |
| 13 | `ChoPhepKetHop: bit` | Cờ cho phép dùng kết hợp với khuyến mãi khác |
| 14 | `DoUuTien: int` | Mức ưu tiên xử lý giữa các khuyến mãi |
| 15 | `GioiHanLuotDung: int` | Giới hạn lượt sử dụng toàn chương trình |
| 16 | `GioiHanMoiKhach: int` | Giới hạn lượt sử dụng của mỗi khách hàng |
| 17 | `NguoiTaoID: long` | Tài khoản tạo chương trình khuyến mãi (FK) |
| 18 | `NgayTao: datetime2(0)` | Thời điểm tạo chương trình |
| 19 | `NgayCapNhat: datetime2(0)` | Thời điểm cập nhật chương trình |
| 20 | `PhienBan: byte[]` | Rowversion phục vụ cập nhật đồng thời |

##### KhuyenMaiSanPham

Bảng liên kết khuyến mãi với các sản phẩm thuộc phạm vi áp dụng.

| STT | Thuộc tính và kiểu | Mô tả |
| --- | --- | --- |
| 1 | `KhuyenMaiID: long` | Chương trình khuyến mãi được gán sản phẩm (PK, FK) |
| 2 | `SanPhamID: long` | Sản phẩm nằm trong phạm vi khuyến mãi (PK, FK) |

##### KhuyenMaiDanhMuc

Bảng liên kết khuyến mãi với các danh mục thuộc phạm vi áp dụng.

| STT | Thuộc tính và kiểu | Mô tả |
| --- | --- | --- |
| 1 | `KhuyenMaiID: long` | Chương trình khuyến mãi được gán danh mục (PK, FK) |
| 2 | `DanhMucID: int` | Danh mục nằm trong phạm vi khuyến mãi (PK, FK) |

##### KhuyenMaiThuongHieu

Bảng liên kết khuyến mãi với các thương hiệu thuộc phạm vi áp dụng.

| STT | Thuộc tính và kiểu | Mô tả |
| --- | --- | --- |
| 1 | `KhuyenMaiID: long` | Chương trình khuyến mãi được gán thương hiệu (PK, FK) |
| 2 | `ThuongHieuID: int` | Thương hiệu nằm trong phạm vi khuyến mãi (PK, FK) |

##### KhuyenMaiDonHang

Lưu chương trình đã áp dụng trên đơn hàng và số tiền được giảm thực tế.

| STT | Thuộc tính và kiểu | Mô tả |
| --- | --- | --- |
| 1 | `DonHangID: long` | Đơn hàng đã áp dụng khuyến mãi (PK, FK) |
| 2 | `KhuyenMaiID: long` | Chương trình được dùng tại thời điểm đặt (PK, FK) |
| 3 | `MaLucApDung: string` | Mã khuyến mãi được lưu tại lúc áp dụng |
| 4 | `TenLucApDung: string` | Tên chương trình lúc áp dụng |
| 5 | `HinhThucLucApDung: string` | Hình thức giảm giá lúc áp dụng |
| 6 | `GiaTriLucApDung: decimal(18,2)` | Giá trị giảm lúc áp dụng |
| 7 | `TienGiamThucTe: decimal(18,2)` | Số tiền giảm thực tế đóng góp vào đơn hàng |
| 8 | `NgayApDung: datetime2(0)` | Thời điểm áp dụng khuyến mãi |

##### LichSuKhuyenMai

Ghi lịch sử tạo, sửa, kích hoạt và kết thúc chương trình khuyến mãi.

| STT | Thuộc tính và kiểu | Mô tả |
| --- | --- | --- |
| 1 | `LichSuID: long` | Mã dòng lịch sử khuyến mãi (PK) |
| 2 | `KhuyenMaiID: long` | Chương trình xảy ra thao tác (FK) |
| 3 | `NguoiThucHienID: long` | Tài khoản thực hiện thao tác (FK) |
| 4 | `HanhDong: varchar(30)` | Loại thao tác: tạo, sửa, kích hoạt hoặc kết thúc |
| 5 | `NoiDung: nvarchar(max)` | Thông tin chi tiết thay đổi |
| 6 | `ThoiDiem: datetime2(0)` | Thời điểm thực hiện thao tác |

##### YeuCauDoiTra

Lưu yêu cầu đổi hoặc trả hàng, trạng thái duyệt và đơn đổi mới nếu có.

| STT | Thuộc tính và kiểu | Mô tả |
| --- | --- | --- |
| 1 | `YeuCauDoiTraID: long` | Mã định danh yêu cầu đổi hoặc trả (PK) |
| 2 | `MaYeuCau: string` | Mã nghiệp vụ của yêu cầu đổi/trả (UK) |
| 3 | `MaChongLap: Guid` | Mã chống ghi nhận yêu cầu bị gửi lại nhiều lần (UK) |
| 4 | `DonHangID: long` | Đơn hàng gốc phát sinh yêu cầu (FK) |
| 5 | `KhachHangID: long` | Khách hàng gửi yêu cầu và là chủ đơn gốc (FK) |
| 6 | `LoaiYeuCau: string` | Loại yêu cầu: DOI hoặc TRA |
| 7 | `LyDo: nvarchar(1000)` | Lý do khách yêu cầu đổi/trả |
| 8 | `TrangThai: string` | Trạng thái xử lý từ chờ duyệt đến hoànattet/từ chối/hủy |
| 9 | `NguoiDuyetID: long` | Tài khoản duyệt yêu cầu (FK) |
| 10 | `NgayTao: datetime2(0)` | Thời điểm tạo yêu cầu |
| 11 | `NgayDuyet: datetime2(0)` | Thời điểm duyệt yêu cầu |
| 12 | `LyDoXuLy: nvarchar(1000)` | Nội dung giải thích việc duyệt hoặc từ chối |
| 13 | `NgayHoanTat: datetime2(0)` | Thời điểm hoàn tất xử lý yêu cầu |
| 14 | `DonHangDoiMoiID: long` | Đơn hàng thay thế được tạo khi đổi sản phẩm (FK) |
| 15 | `PhienBan: byte[]` | Rowversion kiểm soát thay đổi trạng thái |

##### ChiTietDoiTra

Lưu các dòng hàng đổi/trả, số lượng xử lý và khoản hoàn được duyệt.

| STT | Thuộc tính và kiểu | Mô tả |
| --- | --- | --- |
| 1 | `ChiTietDoiTraID: long` | Mã định danh dòng hàng đổi/trả (PK) |
| 2 | `YeuCauDoiTraID: long` | Yêu cầu đổi/trả chứa dòng hàng (FK) |
| 3 | `DonHangID: long` | Mã đơn hàng gốc (tham gia các FK ghép) (FK) |
| 4 | `ChiTietDonHangID: long` | Dòng hàng đã mua đang được đổi/trả (FK) |
| 5 | `SoLuongYeuCau: int` | Số lượng khách đề nghị đổi hoặc trả |
| 6 | `SoLuongDuyet: int` | Số lượng được nhân viên chấp thuận |
| 7 | `SoLuongDaNhan: int` | Số lượng hàng cửa hàng đã nhận lại |
| 8 | `SoLuongBanLai: int` | Số lượng hàng nhận về đủ điều kiện nhập lại kho bán |
| 9 | `TienHoanDuocDuyet: decimal(18,2)` | Số tiền hoàn được duyệt cho dòng hàng |
| 10 | `BienTheDoiSangID: long` | Biến thể khách sẽ nhận khi thực hiện đổi (FK) |
| 11 | `TinhTrangHang: string` | Mô tả tình trạng của hàng đổi/trả |
| 12 | `NguoiNhanHangID: long` | Tài khoản nhân viên nhận hàng (FK) |
| 13 | `NgayNhanHang: datetime2(0)` | Thời điểm cửa hàng nhận hàng |

##### HinhAnhDoiTra

Lưu hình ảnh làm bằng chứng cho yêu cầu đổi/trả.

| STT | Thuộc tính và kiểu | Mô tả |
| --- | --- | --- |
| 1 | `HinhAnhID: long` | Mã ảnh chứng minh đổi/trả (PK) |
| 2 | `YeuCauDoiTraID: long` | Yêu cầu đổi/trả có ảnh đính kèm (FK) |
| 3 | `DuongDan: string` | Đường dẫn hoặc URL ảnh bằng chứng |
| 4 | `MoTa: string` | Mô tả nội dung ảnh |
| 5 | `NgayTao: datetime2(0)` | Thời điểm ảnh được lưu |

##### LichSuDoiTra

Ghi nhận lịch sử xử lý và thay đổi trạng thái yêu cầu đổi/trả.

| STT | Thuộc tính và kiểu | Mô tả |
| --- | --- | --- |
| 1 | `LichSuID: long` | Mã dòng lịch sử đổi/trả (PK) |
| 2 | `YeuCauDoiTraID: long` | Yêu cầu đổi/trả được cập nhật (FK) |
| 3 | `TrangThaiCu: string` | Trạng thái yêu cầu trước thao tác |
| 4 | `TrangThaiMoi: string` | Trạng thái yêu cầu sau thao tác |
| 5 | `NguoiThucHienID: long` | Tài khoản nhân viên hoặc quản trị thực hiện thao tác (FK) |
| 6 | `NoiDung: string` | Lý do hoặc diễn giải thao tác |
| 7 | `ThoiDiem: datetime2(0)` | Thời điểm ghi nhận thao tác |

##### HoanTien

Lưu giao dịch hoàn tiền gắn với khoản thanh toán gốc và yêu cầu đổi/trả (nếu có).

| STT | Thuộc tính và kiểu | Mô tả |
| --- | --- | --- |
| 1 | `HoanTienID: long` | Mã định danh giao dịch hoàn tiền (PK) |
| 2 | `DonHangID: long` | Đơn hàng được hoàn tiền (FK) |
| 3 | `ThanhToanID: long` | Khoản thanh toán gốc nhận hoàn tiền (FK) |
| 4 | `YeuCauDoiTraID: long` | Yêu cầu đổi/trả liên quan, có thể rỗng khi hủy đơn (FK) |
| 5 | `MaYeuCau: uniqueidentifier` | Mã duy nhất chống hoàn tiền nhiều lần do yêu cầu trùng (UK) |
| 6 | `LyDo: string` | Lý do hoàn tiền |
| 7 | `SoTien: decimal(18,2)` | Số tiền hoàn, phải lớn hơn 0 |
| 8 | `TrangThai: string` | Trạng thái xử lý hoàn tiền |
| 9 | `KenhHoanTien: string` | Kênh hoặc cổng thực hiện hoàn tiền |
| 10 | `MaGiaoDichHoan: string` | Mã giao dịch hoàn tiền từ kênh xử lý |
| 11 | `BangChung: string` | Chứng từ xác nhận đã hoàn tiền |
| 12 | `NguoiXuLyID: long` | Tài khoản xử lý việc hoàn tiền (FK) |
| 13 | `NgayTao: datetime2(0)` | Thời điểm tạo giao dịch hoàn |
| 14 | `NgayXacNhan: datetime2(0)` | Thời điểm hoàn tiền được xác nhận thành công |
| 15 | `PhienBan: byte[]` | Rowversion để kiểm soát đồng thời |

##### DanhGia

Lưu đánh giá và điểm số của khách hàng cho từng dòng hàng đã mua.

| STT | Thuộc tính và kiểu | Mô tả |
| --- | --- | --- |
| 1 | `DanhGiaID: long` | Mã định danh đánh giá (PK) |
| 2 | `DonHangID: long` | Đơn hàng phát sinh đánh giá (FK) |
| 3 | `ChiTietDonHangID: long` | Dòng hàng đã mua được đánh giá (tối đa một đánh giá) (FK, UK) |
| 4 | `KhachHangID: long` | Khách hàng gửi đánh giá, phải thuộc đơn hàng (FK) |
| 5 | `SoSao: tinyint` | Điểm đánh giá từ 1 đến 5 sao |
| 6 | `NoiDung: nvarchar(2000)` | Nội dung nhận xét của khách hàng |
| 7 | `TrangThai: string` | Trạng thái hiển thị đánh giá: HIEN_THI hoặc AN |
| 8 | `NgayTao: datetime2(0)` | Thời điểm tạo đánh giá |
| 9 | `NgayCapNhat: datetime2(0)` | Thời điểm chỉnh sửa đánh giá |

## 8. Những điểm cần thống nhất trong bản gốc

- Phần tổng quan ghi ‘ba vai trò’ (CUSTOMER, SHOP_MANAGER, SYSTEM_ADMIN).
- Hai bảng lọc sản phẩm (UC-02B) và xem chi tiết sản phẩm (UC-02C) cùng dùng mã UC-02. Tài liệu Markdown giữ tên để phân biệt hai use case.
- Mục 1.4.3 chuyển từ 1.4.3.19 sang 1.4.3.22. Phần thuộc tính của ChiTietGioHang xuất hiện sau GioHang nhưng thiếu tiêu đề. PhuongThucThanhToan có tên trong danh sách lớp và có mô tả đầy đủ ở mục 1.5.3.
- Phần yêu cầu chức năng ghi xóa danh mục/thương hiệu. Use case chi tiết quy định ẩn khi có liên kết và cấm xóa cứng mục đang dùng.
- Mục lục có một số đề mục không đồng nhất với nội dung chính, gồm các tiểu mục bảo mật. Số liệu trong bản Markdown dựa trên nội dung thực tế.
- Phần giới thiệu nhắc MySQL, nhưng kiểu dữ liệu trong từ điển như nvarchar, datetime2, uniqueidentifier và rowversion thuộc thiết kế SQL Server. Khi đối chiếu với code MySQL, cần xem kiểu dữ liệu tương ứng trong schema triển khai.
- **Lưu ý tương thích phiên bản:** Bảng lược đồ dữ liệu sử dụng kiểu dữ liệu SQL Server (nvarchar, datetime2, uniqueidentifier, rowversion) mặc dù tài liệu có phần giới thiệu MySQL. Cần kiểm tra mapping khi triển khai với MySQL.
- Đánh giá sản phẩm, khách gửi yêu cầu đổi/trả, sửa hồ sơ khách và xem nhật ký được đề cập trong yêu cầu hoặc tổng quan, nhưng chưa có bảng use case độc lập ở mục 1.2.