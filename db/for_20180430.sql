-- for edition at 2018/04/30
alter table hst_action add column json_code text;
alter table hst_group add column json_code text;
alter table hst_remark add column json_code text;
alter table hst_shop add column json_code text;
alter table mst_book add column json_code text;
alter table mst_category add column json_code text;
alter table mst_item add column json_code text;
alter table rel_book_item add column json_code text;
alter table mst_book drop column csv_act_time_index;
alter table mst_book drop column csv_outgo_index;
alter table mst_book drop column csv_item_name_index;